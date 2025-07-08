cmake_minimum_required(VERSION 3.15)
project(whisper_wrapper)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Define the whisper wrapper library
add_library(whisper_wrapper SHARED
    whisper_wrapper.cpp
    whisper_wrapper.hpp
)

# Set the export definition for Windows
if(WIN32)
    target_compile_definitions(whisper_wrapper PRIVATE WHISPER_WRAPPER_EXPORTS)
endif()

# Find OpenVINO GenAI
find_package(OpenVINOGenAI REQUIRED)

# Link against OpenVINO GenAI
target_link_libraries(whisper_wrapper PRIVATE openvino::genai)

# Set the output name to match what our C# code expects
set_target_properties(whisper_wrapper PROPERTIES
    OUTPUT_NAME "openvino_genai_c"
    PREFIX ""
)

# Ensure we copy the output to the appropriate runtime directory
if(WIN32)
    set(RUNTIME_OUTPUT_DIR "${CMAKE_SOURCE_DIR}/../../build/native/runtimes/win-x64/native")
    set_target_properties(whisper_wrapper PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RUNTIME_OUTPUT_DIR}"
        RUNTIME_OUTPUT_DIRECTORY_DEBUG "${RUNTIME_OUTPUT_DIR}"
        RUNTIME_OUTPUT_DIRECTORY_RELEASE "${RUNTIME_OUTPUT_DIR}"
    )
endif()

# Install the library
install(TARGETS whisper_wrapper
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)