cmake_minimum_required(VERSION 3.15)
project(openvino_genai_wrapper)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Define the complete OpenVINO GenAI wrapper library
add_library(openvino_genai_wrapper SHARED
    whisper_wrapper.cpp
    whisper_wrapper.hpp
)

# Set the export definition for Windows
if(WIN32)
    target_compile_definitions(openvino_genai_wrapper PRIVATE WHISPER_WRAPPER_EXPORTS)
endif()

# Find OpenVINO GenAI - enhanced detection with debugging
message(STATUS "Looking for OpenVINO GenAI...")

# Set up search paths based on environment and prefix
set(OPENVINO_GENAI_SEARCH_PATHS)
if(DEFINED ENV{OPENVINO_GENAI_ROOT})
    message(STATUS "Using OPENVINO_GENAI_ROOT: $ENV{OPENVINO_GENAI_ROOT}")
    list(APPEND OPENVINO_GENAI_SEARCH_PATHS "$ENV{OPENVINO_GENAI_ROOT}")
    set(OpenVINO_DIR "$ENV{OPENVINO_GENAI_ROOT}/runtime/cmake")
    set(OpenVINOGenAI_DIR "$ENV{OPENVINO_GENAI_ROOT}/runtime/cmake")
endif()

if(CMAKE_PREFIX_PATH)
    message(STATUS "Using CMAKE_PREFIX_PATH: ${CMAKE_PREFIX_PATH}")
    list(APPEND OPENVINO_GENAI_SEARCH_PATHS "${CMAKE_PREFIX_PATH}")
endif()

# Try to find OpenVINO GenAI package first
find_package(OpenVINOGenAI QUIET)
if(OpenVINOGenAI_FOUND)
    message(STATUS "Found OpenVINO GenAI package")
else()
    message(STATUS "OpenVINO GenAI package not found, trying manual detection...")
    
    # Manual detection with enhanced paths
    set(INCLUDE_SEARCH_PATHS)
    set(LIB_SEARCH_PATHS)
    
    foreach(SEARCH_PATH IN LISTS OPENVINO_GENAI_SEARCH_PATHS)
        list(APPEND INCLUDE_SEARCH_PATHS 
            "${SEARCH_PATH}/runtime/include"
            "${SEARCH_PATH}/include"
        )
        list(APPEND LIB_SEARCH_PATHS
            "${SEARCH_PATH}/runtime/lib"
            "${SEARCH_PATH}/lib"
            "${SEARCH_PATH}/runtime/bin/intel64/Release"
            "${SEARCH_PATH}/bin/intel64/Release"
        )
    endforeach()
    
    # Debug: Print search paths
    message(STATUS "Include search paths: ${INCLUDE_SEARCH_PATHS}")
    message(STATUS "Library search paths: ${LIB_SEARCH_PATHS}")
    
    # Find header files
    find_path(OPENVINO_GENAI_INCLUDE_DIR 
        NAMES openvino/genai/whisper_pipeline.hpp
              openvino/genai/llm_pipeline.hpp
              openvino/genai/generation_config.hpp
        PATHS ${INCLUDE_SEARCH_PATHS}
        NO_DEFAULT_PATH)
    
    # Debug: Show what was found
    message(STATUS "OPENVINO_GENAI_INCLUDE_DIR result: ${OPENVINO_GENAI_INCLUDE_DIR}")
    
    if(OPENVINO_GENAI_INCLUDE_DIR)
        message(STATUS "Found OpenVINO GenAI headers: ${OPENVINO_GENAI_INCLUDE_DIR}")
    else()
        # Let's try to find the files manually for debugging
        foreach(search_path ${INCLUDE_SEARCH_PATHS})
            message(STATUS "Checking path: ${search_path}")
            if(EXISTS "${search_path}/openvino/genai/whisper_pipeline.hpp")
                message(STATUS "  Found whisper_pipeline.hpp in ${search_path}")
            endif()
            if(EXISTS "${search_path}/openvino/genai/llm_pipeline.hpp")
                message(STATUS "  Found llm_pipeline.hpp in ${search_path}")
            endif()
            if(EXISTS "${search_path}/openvino/genai/generation_config.hpp")
                message(STATUS "  Found generation_config.hpp in ${search_path}")
            endif()
        endforeach()
        message(FATAL_ERROR "Could not find OpenVINO GenAI headers")
    endif()
    
    # Find library files
    find_library(OPENVINO_GENAI_LIBRARY 
        NAMES openvino_genai
        PATHS ${LIB_SEARCH_PATHS}
        NO_DEFAULT_PATH)
    
    if(OPENVINO_GENAI_LIBRARY)
        message(STATUS "Found OpenVINO GenAI library: ${OPENVINO_GENAI_LIBRARY}")
    else()
        message(FATAL_ERROR "Could not find OpenVINO GenAI library")
    endif()
    
    # Find additional dependencies
    find_library(OPENVINO_CORE_LIBRARY 
        NAMES openvino
        PATHS ${LIB_SEARCH_PATHS}
        NO_DEFAULT_PATH)
    
    if(OPENVINO_CORE_LIBRARY)
        message(STATUS "Found OpenVINO Core library: ${OPENVINO_CORE_LIBRARY}")
    endif()
    
    # Create imported target
    add_library(openvino::genai UNKNOWN IMPORTED)
    set_target_properties(openvino::genai PROPERTIES
        IMPORTED_LOCATION "${OPENVINO_GENAI_LIBRARY}"
        INTERFACE_INCLUDE_DIRECTORIES "${OPENVINO_GENAI_INCLUDE_DIR}")
    
    # Add OpenVINO core dependency if found
    if(OPENVINO_CORE_LIBRARY)
        add_library(openvino::runtime UNKNOWN IMPORTED)
        set_target_properties(openvino::runtime PROPERTIES
            IMPORTED_LOCATION "${OPENVINO_CORE_LIBRARY}")
        target_link_libraries(openvino::genai INTERFACE openvino::runtime)
    endif()
    
    message(STATUS "OpenVINO GenAI configured manually")
endif()

# Link against OpenVINO GenAI
target_link_libraries(openvino_genai_wrapper PRIVATE openvino::genai)

# Ensure include directories are properly set
target_include_directories(openvino_genai_wrapper PRIVATE 
    ${CMAKE_CURRENT_SOURCE_DIR})

# Set the output name to match what our C# code expects
set_target_properties(openvino_genai_wrapper PROPERTIES
    OUTPUT_NAME "openvino_genai_c"
    PREFIX ""
)

# Ensure we copy the output to the appropriate runtime directory
if(WIN32)
    set(RUNTIME_OUTPUT_DIR "${CMAKE_SOURCE_DIR}/../../build/native/runtimes/win-x64/native")
    set_target_properties(openvino_genai_wrapper PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${RUNTIME_OUTPUT_DIR}"
        RUNTIME_OUTPUT_DIRECTORY_DEBUG "${RUNTIME_OUTPUT_DIR}"
        RUNTIME_OUTPUT_DIRECTORY_RELEASE "${RUNTIME_OUTPUT_DIR}"
    )
endif()

# Install the library
install(TARGETS openvino_genai_wrapper
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)