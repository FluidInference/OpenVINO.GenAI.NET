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

# Find OpenVINO GenAI - try different approaches
if(DEFINED ENV{OPENVINO_GENAI_ROOT})
    set(OpenVINO_DIR "$ENV{OPENVINO_GENAI_ROOT}/runtime/cmake")
    set(OpenVINOGenAI_DIR "$ENV{OPENVINO_GENAI_ROOT}/runtime/cmake")
endif()

find_package(OpenVINOGenAI QUIET)
if(NOT OpenVINOGenAI_FOUND)
    # Fallback: try to find headers and libraries manually
    find_path(OPENVINO_GENAI_INCLUDE_DIR 
        NAMES openvino/genai/whisper_pipeline.hpp
        PATHS ${CMAKE_PREFIX_PATH}/runtime/include
              ${CMAKE_PREFIX_PATH}/include
        NO_DEFAULT_PATH)
    
    find_library(OPENVINO_GENAI_LIBRARY 
        NAMES openvino_genai
        PATHS ${CMAKE_PREFIX_PATH}/runtime/lib
              ${CMAKE_PREFIX_PATH}/lib
        NO_DEFAULT_PATH)
    
    if(OPENVINO_GENAI_INCLUDE_DIR AND OPENVINO_GENAI_LIBRARY)
        add_library(openvino::genai UNKNOWN IMPORTED)
        set_target_properties(openvino::genai PROPERTIES
            IMPORTED_LOCATION "${OPENVINO_GENAI_LIBRARY}"
            INTERFACE_INCLUDE_DIRECTORIES "${OPENVINO_GENAI_INCLUDE_DIR}")
        message(STATUS "Found OpenVINO GenAI manually: ${OPENVINO_GENAI_LIBRARY}")
    else()
        message(FATAL_ERROR "Could not find OpenVINO GenAI")
    endif()
endif()

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