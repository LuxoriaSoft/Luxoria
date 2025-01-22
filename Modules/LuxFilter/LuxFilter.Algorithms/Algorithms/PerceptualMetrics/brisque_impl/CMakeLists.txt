cmake_minimum_required(VERSION 3.23)
project(BRISQUEQuality)

# Set C++23 standard
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Set OpenCV_DIR to the location of your OpenCV build directory
# Update this path to where your OpenCVConfig.cmake file is located
set(OpenCV_DIR "./ext_libs/opencv/build")

# Find the OpenCV library
find_package(OpenCV REQUIRED)

# Add the executable
add_executable(brisque_quality main.cpp)

# Add the includes folder to the include directories
target_include_directories(brisque_quality PUBLIC ${CMAKE_SOURCE_DIR}/includes)

# Link the OpenCV libraries
target_link_libraries(brisque_quality ${OpenCV_LIBS})

# Optional: Add include directories for OpenCV headers
include_directories(${OpenCV_INCLUDE_DIRS})

# Set the output directory for the built executable
set_target_properties(brisque_quality PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}"
)
