#include <opencv2/imgproc.hpp>

#include <string>
#include <filesystem>
#include <iostream>

#include "BrisqueAlgorithm.hpp"

#define DEFAULT_MODEL_PATH "./models/brisque_model_live.yml"
#define DEFAULT_RANGE_PATH "./models/brisque_range_live.yml"


int main(int ac, char **av) {
    if (ac < 2) {
        std::cerr << "Usage: " << av[0] << " <image_path> [model_path] [range_path]" << std::endl;
        return 1;
    }

    // Load the input image
    std::string imagePath = av[1];
    cv::Mat img = cv::imread(imagePath, cv::IMREAD_COLOR);

    if (img.empty()) {
        std::cerr << "Error: Unable to load image at " << imagePath << std::endl;
        return -1;
    }

    // Determine paths to model and range files
    const char* modelPath = (ac > 2) ? av[2] : DEFAULT_MODEL_PATH;
    const char* rangePath = (ac > 3) ? av[3] : DEFAULT_RANGE_PATH;

    // Check if model and range files exist
    if (!std::filesystem::exists(modelPath)) {
        std::cerr << "Error: Model file not found at " << modelPath << std::endl;
        return -1;
    }
    if (!std::filesystem::exists(rangePath)) {
        std::cerr << "Error: Range file not found at " << rangePath << std::endl;
        return -1;
    }

    // Convert image to grayscale
    cv::Mat grayImg;
    cv::cvtColor(img, grayImg, cv::COLOR_BGR2GRAY);

    // Create a BRISQUE algorithm instance
    luxoria::filter::algorithms::BrisqueAlgorithm brisque(modelPath, rangePath);

    try {
        // Compute BRISQUE score
        std::cout << "Computing BRISQUE score..." << std::endl;
        double scoreInstance = brisque.compute(grayImg);
        std::cout << "BRISQUE Score: " << scoreInstance << std::endl;
    } catch (const cv::Exception& e) {
        std::cerr << "Error during BRISQUE computation: " << e.what() << std::endl;
        return -1;
    }

    return 0;
}
