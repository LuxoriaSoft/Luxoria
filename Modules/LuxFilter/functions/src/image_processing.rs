// LuxFilter Image Processing Module
// This module contains functions for image processing
// Such as Apply grayscale

use image::{DynamicImage, RgbaImage, Luma};

#[no_mangle]
pub extern "C" fn apply_grayscale(bitmap: *const u8, width: u32, height: u32) -> *mut u8 {
    // Safety: We are assuming the bitmap pointer is valid and points to a buffer of width * height * 4 bytes (RGBA)
    let bitmap = unsafe { std::slice::from_raw_parts(bitmap, (width * height * 4) as usize) };

    // Load the image from raw bytes (RGBA format)
    let img = DynamicImage::from_rgba8(width, height, bitmap.to_vec());

    // Convert to grayscale (Luma8)
    let gray_img = img.to_luma8();

    // Allocate a new vector to hold the grayscale image
    let gray_bytes = gray_img.to_vec();

    // Allocate memory on the heap for the resulting grayscale bitmap
    let result_ptr = gray_bytes.as_mut_ptr();
    
    // The resulting vector needs to be kept alive manually, so we must ensure the memory is freed later
    std::mem::forget(gray_bytes);

    // Return the pointer to the grayscale bitmap
    result_ptr
}

// Function to free the memory of the returned bitmap
#[no_mangle]
pub extern "C" fn free_bitmap(ptr: *mut u8) {
    if !ptr.is_null() {
        unsafe {
            Box::from_raw(ptr); // This will drop and free the memory
        }
    }
}