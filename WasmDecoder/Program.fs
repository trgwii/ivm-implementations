open System

[<EntryPoint>]
let main argv =
    // (module
    //   (type $t0 (func (param i32 i32) (result i32)))
    //   (func $add (export "add") (type $t0) (param $lhs i32) (param $rhs i32) (result i32)
    //     get_local $lhs
    //     get_local $rhs
    //     i32.add))

    let program = [| 0x00; 0x61; 0x73; 0x6d; 0x01; 0x00; 0x00; 0x00; 0x01; 0x07; 0x01; 0x60; 0x02; 0x7f; 0x7f; 0x01;
                     0x7f; 0x03; 0x02; 0x01; 0x00; 0x07; 0x07; 0x01; 0x03; 0x61; 0x64; 0x64; 0x00; 0x00; 0x0a; 0x09;
                     0x01; 0x07; 0x00; 0x20; 0x00; 0x20; 0x01; 0x6a; 0x0b; 0x00; 0x1c; 0x04; 0x6e; 0x61; 0x6d; 0x65;
                     0x01; 0x06; 0x01; 0x00; 0x03; 0x61; 0x64; 0x64; 0x02; 0x0d; 0x01; 0x00; 0x02; 0x00; 0x03; 0x6c;
                     0x68; 0x73; 0x01; 0x03; 0x72; 0x68; 0x73; |]

    let m = Decode.decode (Array.map byte program)

    0 // return an integer exit code
