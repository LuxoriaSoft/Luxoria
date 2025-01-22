#include <opencv2/quality.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/imgproc.hpp>

#pragma once

namespace luxoria {
    namespace filter
    {
        namespace algorithms
        {
            // Class definition for BrisqueAlgorithm
            // This class encapsulates the BRISQUE (Blind/Referenceless Image Spatial Quality Evaluator) algorithm
            class BrisqueAlgorithm {
                private:
                    cv::Ptr<cv::quality::QualityBRISQUE> _model;
                public:
                    // Constructor
                    // Initializes the BRISQUE model using the specified model and range files
                    BrisqueAlgorithm(std::string modelPath, std::string rangePath) {
                        // Load the BRISQUE model and range files
                        _model = cv::quality::QualityBRISQUE::create(modelPath, rangePath);
                    }

                    // Destructor
                    // Releases resources held by the BRISQUE model
                    ~BrisqueAlgorithm() {
                        _model.release();
                    }

                    // Method to compute the BRISQUE score for a given image
                    // @param img: Input image (cv::Mat)
                    // @return: A double representing the BRISQUE score
                    double compute(cv::Mat img) {
                        cv::Scalar scoreInstance = _model->compute(img);  // Compute the quality score
                        return scoreInstance[0];  // Return the first element as the BRISQUE score
                    }
            };
        }
    }
}
