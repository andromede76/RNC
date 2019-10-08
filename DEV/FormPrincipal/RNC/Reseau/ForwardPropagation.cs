using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RNC.Neurones;

namespace RNC.Reseau
{
    public class ForwardPropagation
    {
        public ReseauNeurone _RNN;
        public int currentPatternIndex;

        public const int GAUSSIAN_FIELD_SIZE = 21;

        public static int i;

        double[,] _GaussianKernel = new double[GAUSSIAN_FIELD_SIZE, GAUSSIAN_FIELD_SIZE];

        protected int colonne;  // size of the distortion maps
        protected int ligne;

        protected double[] m_DispH;  // horiz distortion map array
        protected double[] m_DispV;  // vert distortion map array

        // parameters for controlling distortions of input image

        double MaxScaling = 15.0;  // like 20.0 for 20%
        double MaxRotation = 15.0;  // like 20.0 for 20 degrees
        double ElasticSigma = 8.0;  // higher numbers are more smooth and less distorted; Simard uses 4.0
        double ElasticScaling = 0.5;  // higher numbers amplify the distortions; Simard uses 34 (sic, maybe 0.34 ??)

        public ForwardPropagation()
        {
            currentPatternIndex = 0;
            _RNN = null;
            ligne = 29;
            colonne = 29;

            m_DispH = new double[ligne*colonne];
            m_DispV = new double[ligne * colonne];


        }
        /// <summary>
        ///  Création d'un kernel gaussien, lequel est constant, pour générer des distorsions elastiques
        /// </summary>
        /// <param name="_dElasticSigma"></param>
        protected void GetGaussianKernel(double _dElasticSigma)
        {
            int iiMid = GAUSSIAN_FIELD_SIZE / 2;

            double twoSigmaSquared = 2.0 * (_dElasticSigma) * (_dElasticSigma);
            twoSigmaSquared = 1.0 / twoSigmaSquared;
            double twoPiSigma = 1.0 / (_dElasticSigma) * Math.Sqrt(2.0 * Math.PI);

            for (int col = 0; col < GAUSSIAN_FIELD_SIZE; ++col)
            {
                for (int row = 0; row < GAUSSIAN_FIELD_SIZE; ++row)
                {
                    _GaussianKernel[row, col] = twoPiSigma *
                        (Math.Exp(-(((row - iiMid) * (row - iiMid) + (col - iiMid) * (col - iiMid)) * twoSigmaSquared)));
                }
            }
        }
        public void CalculateNeuralNet(double[] inputVector, double[] cibleVector, double[] courantVector, NeuroneSortieListe neuroneSortieListe, bool Distorsion)
        {

            _RNN.etat = i++;

            if (Distorsion != false)
            {
                GenerateDistortionMap(1.0);
                ApplyDistortionMap(inputVector);
            }

            _RNN.Calculate(inputVector, cibleVector, courantVector, neuroneSortieListe);
        }

        public void CalculateNeuralNetTest(double[] inputVector, double[] cibleVector, double[] courantVector, NeuroneSortieListe neuroneSortieListe)
        {

            _RNN.etat = i++;

           

            _RNN.CalculateTest(inputVector, cibleVector, courantVector, neuroneSortieListe);
        }

        #region ApplyDistorsionMap
        protected void ApplyDistortionMap(double[] inputVector)
        {
            // applies the current distortion map to the input vector

            // For the mapped array, we assume that 0.0 == background, and 1.0 == full intensity information
            // This is different from the input vector, in which +1.0 == background (white), and 
            // -1.0 == information (black), so we must convert one to the other

            List<List<double>> mappedVector = new List<List<double>>(ligne);
            for (int i = 0; i < ligne; i++)
            {
                List<double> mVector = new List<double>(colonne);

                for (int j = 0; j < colonne; j++)
                {
                    mVector.Add(0.0);
                }
                mappedVector.Add(mVector);
            }

            double sourceRow, sourceCol;
            double fracRow, fracCol;
            double w1, w2, w3, w4;
            double sourceValue;
            int row, col;
            int sRow, sCol, sRowp1, sColp1;
            bool bSkipOutOfBounds;

            for (row = 0; row < ligne; ++row)
            {
                for (col = 0; col < colonne; ++col)
                {
                    // the pixel at sourceRow, sourceCol is an "phantom" pixel that doesn't really exist, and
                    // whose value must be manufactured from surrounding real pixels (i.e., since 
                    // sourceRow and sourceCol are floating point, not ints, there's not a real pixel there)
                    // The idea is that if we can calculate the value of this phantom pixel, then its 
                    // displacement will exactly fit into the current pixel at row, col (which are both ints)

                    sourceRow = (double)row - m_DispV[row * colonne + col];
                    sourceCol = (double)col - m_DispH[row * colonne + col];

                    // weights for bi-linear interpolation

                    fracRow = sourceRow - (int)sourceRow;
                    fracCol = sourceCol - (int)sourceCol;


                    w1 = (1.0 - fracRow) * (1.0 - fracCol);
                    w2 = (1.0 - fracRow) * fracCol;
                    w3 = fracRow * (1 - fracCol);
                    w4 = fracRow * fracCol;


                    // limit indexes

                    /*
                                while (sourceRow >= m_cRows ) sourceRow -= m_cRows;
                                while (sourceRow < 0 ) sourceRow += m_cRows;
			
                                while (sourceCol >= m_cCols ) sourceCol -= m_cCols;
                                while (sourceCol < 0 ) sourceCol += m_cCols;
                    */
                    bSkipOutOfBounds = false;

                    if ((sourceRow + 1.0) >= ligne) bSkipOutOfBounds = true;
                    if (sourceRow < 0) bSkipOutOfBounds = true;

                    if ((sourceCol + 1.0) >= colonne) bSkipOutOfBounds = true;
                    if (sourceCol < 0) bSkipOutOfBounds = true;

                    if (bSkipOutOfBounds == false)
                    {
                        // the supporting pixels for the "phantom" source pixel are all within the 
                        // bounds of the character grid.
                        // Manufacture its value by bi-linear interpolation of surrounding pixels

                        sRow = (int)sourceRow;
                        sCol = (int)sourceCol;

                        sRowp1 = sRow + 1;
                        sColp1 = sCol + 1;

                        while (sRowp1 >= ligne) sRowp1 -= ligne;
                        while (sRowp1 < 0) sRowp1 += ligne;

                        while (sColp1 >= colonne) sColp1 -= colonne;
                        while (sColp1 < 0) sColp1 += colonne;

                        // perform bi-linear interpolation

                        sourceValue = w1 * inputVector[sRow * colonne + sCol] +
                            w2 * w1 * inputVector[sRow * colonne + sColp1] +
                            w3 * w1 * inputVector[sRowp1 * colonne + sCol] +
                            w4 * w1 * inputVector[sRowp1 * colonne + sColp1];
                    }
                    else
                    {
                        // At least one supporting pixel for the "phantom" pixel is outside the
                        // bounds of the character grid. Set its value to "background"

                        sourceValue = 1.0;  // "background" color in the -1 -> +1 range of inputVector
                    }

                    mappedVector[row][col] = 0.5 * (1.0 - sourceValue);  // conversion to 0->1 range we are using for mappedVector

                }
            }

            // now, invert again while copying back into original vector

            for (row = 0; row < ligne; ++row)
            {
                for (col = 0; col < colonne; ++col)
                {
                    inputVector[row * colonne + col] = 1.0 - 2.0 * mappedVector[row][col];
                }
            }

        }

        #endregion 


        protected void GenerateDistortionMap(double severityFactor /* =1.0 */ )
        {
            // generates distortion maps in each of the horizontal and vertical directions
            // Three distortions are applied: a scaling, a rotation, and an elastic distortion
            // Since these are all linear tranformations, we can simply add them together, after calculation
            // one at a time

            // The input parameter, severityFactor, let's us control the severity of the distortions relative
            // to the default values.  For example, if we only want half as harsh a distortion, set
            // severityFactor == 0.5

            // First, elastic distortion, per Patrice Simard, "Best Practices For Convolutional Neural Networks..."
            // at page 2.
            // Three-step process: seed array with uniform randoms, filter with a gaussian kernel, normalize (scale)

            int row, col;
            double[] uniformH = new double[ligne*colonne];
            double[] uniformV = new double[ligne*colonne];
            Random rdm = new Random();

            for (col = 0; col < colonne; ++col)
            {
                for (row = 0; row < ligne; ++row)
                {

                    uniformH[row * colonne + col] = (double)(2.0 * rdm.NextDouble() - 1.0);
                    uniformV[row * colonne + col] = (double)(2.0 * rdm.NextDouble() - 1.0);
                }
            }

            // filter with gaussian

            double fConvolvedH, fConvolvedV;
            double fSampleH, fSampleV;
            double elasticScale = severityFactor * ElasticScaling;
            int xxx, yyy, xxxDisp, yyyDisp;
            int iiMid = GAUSSIAN_FIELD_SIZE / 2;  // GAUSSIAN_FIELD_SIZE (21) is strictly odd

            for (col = 0; col < colonne; ++col)
            {
                for (row = 0; row < ligne; ++row)
                {
                    fConvolvedH = 0.0;
                    fConvolvedV = 0.0;

                    for (xxx = 0; xxx < GAUSSIAN_FIELD_SIZE; ++xxx)
                    {
                        for (yyy = 0; yyy < GAUSSIAN_FIELD_SIZE; ++yyy)
                        {
                            xxxDisp = col - iiMid + xxx;
                            yyyDisp = row - iiMid + yyy;

                            if (xxxDisp < 0 || xxxDisp >= colonne || yyyDisp < 0 || yyyDisp >= ligne)
                            {
                                fSampleH = 0.0;
                                fSampleV = 0.0;
                            }
                            else
                            {
                                fSampleH = uniformH[yyyDisp * colonne + xxxDisp];
                                fSampleV = uniformV[yyyDisp * colonne + xxxDisp];
                            }

                            fConvolvedH += fSampleH * _GaussianKernel[yyy, xxx];
                            fConvolvedV += fSampleV * _GaussianKernel[yyy, xxx];
                        }
                    }

                    m_DispH[row * colonne + col] = elasticScale * fConvolvedH;
                    m_DispV[row * colonne + col] = elasticScale * fConvolvedV;
                }
            }

            uniformH = null;
            uniformV = null;

            // next, the scaling of the image by a random scale factor
            // Horizontal and vertical directions are scaled independently

            double dSFHoriz = severityFactor * MaxScaling / 100.0 * (2.0 * rdm.NextDouble() - 1.0);  // m_dMaxScaling is a percentage
            double dSFVert = severityFactor * MaxScaling / 100.0 * (2.0 * rdm.NextDouble() - 1.0);  // m_dMaxScaling is a percentage


            int iMid = ligne / 2;

            for (row = 0; row < ligne; ++row)
            {
                for (col = 0; col < colonne; ++col)
                {
                    m_DispH[row * colonne + col] = m_DispH[row * colonne + col] + dSFHoriz * (col - iMid);
                    m_DispV[row * colonne + col] = m_DispV[row * colonne + col] - dSFVert * (iMid - row);  // negative because of top-down bitmap
                }
            }


            // finally, apply a rotation

            double angle = severityFactor * MaxRotation * (2.0 * rdm.NextDouble() - 1.0);
            angle = angle * 3.1415926535897932384626433832795 / 180.0;  // convert from degrees to radians

            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            for (row = 0; row < ligne; ++row)
            {
                for (col = 0; col < colonne; ++col)
                {
                    m_DispH[row * colonne + col] = m_DispH[row * colonne + col] + (col - iMid) * (cosAngle - 1) - (iMid - row) * sinAngle;
                    m_DispV[row * colonne + col] = m_DispV[row * colonne + col] - (iMid - row) * (cosAngle - 1) + (col - iMid) * sinAngle;  // negative because of top-down bitmap
                }
            }

        }
    }
}
