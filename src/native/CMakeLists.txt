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

# Try to find OpenVINO GenAI package first with enhanced debugging
message(STATUS "Attempting to find OpenVINO GenAI package...")
message(STATUS "CMAKE_PREFIX_PATH: ${CMAKE_PREFIX_PATH}")
message(STATUS "OpenVINOGenAI_DIR: ${OpenVINOGenAI_DIR}")

find_package(OpenVINOGenAI QUIET)
if(OpenVINOGenAI_FOUND)
    message(STATUS "✅ Found OpenVINO GenAI package successfully!")
    message(STATUS "OpenVINOGenAI_VERSION: ${OpenVINOGenAI_VERSION}")
    message(STATUS "OpenVINOGenAI_INCLUDE_DIRS: ${OpenVINOGenAI_INCLUDE_DIRS}")
    message(STATUS "OpenVINOGenAI_LIBRARIES: ${OpenVINOGenAI_LIBRARIES}")
    
    # Check if the package actually provides the target we need
    if(TARGET openvino::genai)
        message(STATUS "✅ openvino::genai target is available")
        set(OPENVINO_GENAI_FOUND TRUE)
    else()
        message(STATUS "❌ openvino::genai target not found, falling back to manual detection")
        set(OPENVINO_GENAI_FOUND FALSE)
    endif()
else()
    message(STATUS "❌ OpenVINO GenAI package not found, trying manual detection...")
    set(OPENVINO_GENAI_FOUND FALSE)
endif()

if(NOT OPENVINO_GENAI_FOUND)
    
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
    
    # Find header files - Use manual approach since find_path isn't working
    set(OPENVINO_GENAI_INCLUDE_DIR "")
    foreach(search_path ${INCLUDE_SEARCH_PATHS})
        if(EXISTS "${search_path}/openvino/genai/whisper_pipeline.hpp" AND
           EXISTS "${search_path}/openvino/genai/llm_pipeline.hpp" AND
           EXISTS "${search_path}/openvino/genai/generation_config.hpp")
            set(OPENVINO_GENAI_INCLUDE_DIR ${search_path})
            break()
        endif()
    endforeach()
    
    if(OPENVINO_GENAI_INCLUDE_DIR)
        message(STATUS "Found OpenVINO GenAI headers: ${OPENVINO_GENAI_INCLUDE_DIR}")
    else()
        message(FATAL_ERROR "Could not find OpenVINO GenAI headers")
    endif()
    
    # Find library files - Use manual approach with extensive debugging
    set(OPENVINO_GENAI_LIBRARY "")
    
    # Debug: Comprehensive directory exploration to find library files
    message(STATUS "=== COMPREHENSIVE LIBRARY SEARCH ===")
    
    # First, recursively search for ALL .lib files in the OpenVINO GenAI directory
    foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
        if(EXISTS "${base_path}")
            message(STATUS "Recursively searching for .lib files in: ${base_path}")
            file(GLOB_RECURSE all_lib_files "${base_path}/**/*.lib")
            if(all_lib_files)
                message(STATUS "  Found .lib files:")
                foreach(lib_file ${all_lib_files})
                    file(RELATIVE_PATH rel_path "${base_path}" "${lib_file}")
                    message(STATUS "    - ${rel_path}")
                endforeach()
            else()
                message(STATUS "  No .lib files found anywhere in ${base_path}")
            endif()
        endif()
    endforeach()
    
    # Also check for .dll files (in case we need to use them directly)
    message(STATUS "=== CHECKING FOR DLL FILES ===")
    foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
        if(EXISTS "${base_path}")
            message(STATUS "Recursively searching for .dll files in: ${base_path}")
            file(GLOB_RECURSE all_dll_files "${base_path}/**/*.dll")
            if(all_dll_files)
                message(STATUS "  Found .dll files:")
                foreach(dll_file ${all_dll_files})
                    file(RELATIVE_PATH rel_path "${base_path}" "${dll_file}")
                    message(STATUS "    - ${rel_path}")
                endforeach()
            else()
                message(STATUS "  No .dll files found anywhere in ${base_path}")
            endif()
        endif()
    endforeach()
    
    # Debug: List all .lib files in original search paths
    message(STATUS "=== ORIGINAL SEARCH PATHS ===")
    foreach(search_path ${LIB_SEARCH_PATHS})
        message(STATUS "Checking library path: ${search_path}")
        if(EXISTS "${search_path}")
            file(GLOB lib_files "${search_path}/*.lib")
            if(lib_files)
                message(STATUS "  Found .lib files:")
                foreach(lib_file ${lib_files})
                    get_filename_component(lib_name ${lib_file} NAME)
                    message(STATUS "    - ${lib_name}")
                endforeach()
            else()
                message(STATUS "  No .lib files found in ${search_path}")
            endif()
        else()
            message(STATUS "  Path does not exist: ${search_path}")
        endif()
    endforeach()
    
    # Try multiple possible library names in original search paths
    set(POSSIBLE_GENAI_NAMES 
        "openvino_genai.lib"
        "openvino_genai_c.lib"
        "genai.lib"
        "libopenvino_genai.lib"
    )
    
    message(STATUS "=== SEARCHING FOR LIBRARIES BY NAME ===")
    foreach(search_path ${LIB_SEARCH_PATHS})
        foreach(lib_name ${POSSIBLE_GENAI_NAMES})
            set(full_lib_path "${search_path}/${lib_name}")
            if(EXISTS "${full_lib_path}")
                set(OPENVINO_GENAI_LIBRARY "${full_lib_path}")
                message(STATUS "Found OpenVINO GenAI library: ${OPENVINO_GENAI_LIBRARY}")
                break()
            endif()
        endforeach()
        if(OPENVINO_GENAI_LIBRARY)
            break()
        endif()
    endforeach()
    
    # If not found in standard paths, search recursively in the entire OpenVINO GenAI directory
    if(NOT OPENVINO_GENAI_LIBRARY)
        message(STATUS "=== RECURSIVE SEARCH FOR GENAI LIBRARIES ===")
        foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
            foreach(lib_name ${POSSIBLE_GENAI_NAMES})
                if(EXISTS "${base_path}")
                    file(GLOB_RECURSE found_libs "${base_path}/**/${lib_name}")
                    if(found_libs)
                        list(GET found_libs 0 OPENVINO_GENAI_LIBRARY)
                        message(STATUS "Found OpenVINO GenAI library recursively: ${OPENVINO_GENAI_LIBRARY}")
                        break()
                    endif()
                endif()
            endforeach()
            if(OPENVINO_GENAI_LIBRARY)
                break()
            endif()
        endforeach()
    endif()
    
    # Final fallback: try to find any library file with "genai" in the name
    if(NOT OPENVINO_GENAI_LIBRARY)
        message(STATUS "=== FALLBACK: SEARCHING FOR ANY GENAI LIBRARY ===")
        foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
            if(EXISTS "${base_path}")
                file(GLOB_RECURSE genai_libs "${base_path}/**/*genai*.lib")
                if(genai_libs)
                    list(GET genai_libs 0 OPENVINO_GENAI_LIBRARY)
                    message(STATUS "Found GenAI library (fallback): ${OPENVINO_GENAI_LIBRARY}")
                    break()
                endif()
            endif()
        endforeach()
    endif()
    
    if(NOT OPENVINO_GENAI_LIBRARY)
        message(FATAL_ERROR "Could not find OpenVINO GenAI library. Searched for: ${POSSIBLE_GENAI_NAMES}")
    endif()
    
    # Find additional dependencies - Use manual approach with multiple names
    set(OPENVINO_CORE_LIBRARY "")
    
    # Try multiple possible OpenVINO core library names
    set(POSSIBLE_CORE_NAMES 
        "openvino.lib"
        "openvino_c.lib"
        "libopenvino.lib"
        "openvino_runtime.lib"
    )
    
    message(STATUS "=== SEARCHING FOR OPENVINO CORE LIBRARIES ===")
    foreach(search_path ${LIB_SEARCH_PATHS})
        foreach(lib_name ${POSSIBLE_CORE_NAMES})
            set(full_lib_path "${search_path}/${lib_name}")
            if(EXISTS "${full_lib_path}")
                set(OPENVINO_CORE_LIBRARY "${full_lib_path}")
                message(STATUS "Found OpenVINO Core library: ${OPENVINO_CORE_LIBRARY}")
                break()
            endif()
        endforeach()
        if(OPENVINO_CORE_LIBRARY)
            break()
        endif()
    endforeach()
    
    # If not found in standard paths, search recursively
    if(NOT OPENVINO_CORE_LIBRARY)
        message(STATUS "=== RECURSIVE SEARCH FOR OPENVINO CORE LIBRARIES ===")
        foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
            foreach(lib_name ${POSSIBLE_CORE_NAMES})
                if(EXISTS "${base_path}")
                    file(GLOB_RECURSE found_libs "${base_path}/**/${lib_name}")
                    if(found_libs)
                        list(GET found_libs 0 OPENVINO_CORE_LIBRARY)
                        message(STATUS "Found OpenVINO Core library recursively: ${OPENVINO_CORE_LIBRARY}")
                        break()
                    endif()
                endif()
            endforeach()
            if(OPENVINO_CORE_LIBRARY)
                break()
            endif()
        endforeach()
    endif()
    
    # Final fallback: try to find any library file with "openvino" in the name (excluding genai)
    if(NOT OPENVINO_CORE_LIBRARY)
        message(STATUS "=== FALLBACK: SEARCHING FOR ANY OPENVINO LIBRARY ===")
        foreach(base_path ${OPENVINO_GENAI_SEARCH_PATHS})
            if(EXISTS "${base_path}")
                file(GLOB_RECURSE openvino_libs "${base_path}/**/*openvino*.lib")
                if(openvino_libs)
                    # Filter out genai libraries
                    foreach(lib_file ${openvino_libs})
                        get_filename_component(lib_name ${lib_file} NAME)
                        if(NOT lib_name MATCHES "genai")
                            set(OPENVINO_CORE_LIBRARY "${lib_file}")
                            message(STATUS "Found OpenVINO Core library (fallback): ${OPENVINO_CORE_LIBRARY}")
                            break()
                        endif()
                    endforeach()
                    if(OPENVINO_CORE_LIBRARY)
                        break()
                    endif()
                endif()
            endif()
        endforeach()
    endif()
    
    if(NOT OPENVINO_CORE_LIBRARY)
        message(WARNING "Could not find OpenVINO Core library. Searched for: ${POSSIBLE_CORE_NAMES}")
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

# Final configuration - ensure we have the target
if(NOT TARGET openvino::genai)
    message(FATAL_ERROR "Failed to create openvino::genai target. The OpenVINO GenAI package may be incomplete or corrupted.")
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