#![crate_type = "lib"]
#[no_mangle]
pub fn add(a: u32, b: u32) -> u32 {
    return a + b;
}
