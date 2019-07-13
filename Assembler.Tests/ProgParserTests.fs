﻿module Assembler.Tests.ProgParserTests

open System
open Expecto
open Expecto.Impl
open FsCheck
open Swensen.Unquote

open FParsec
open Assembler.Ast
open Assembler.Parser


let parse (progString: string) =
    let bytes = System.Text.Encoding.UTF8.GetBytes(progString)
    use stream = new System.IO.MemoryStream(bytes)
    parseProgram stream |> fst

// Opposite order of Expect.* (for convenience).
let success expected progString () =
    let result = try parse progString with ParseException(msg) -> failtest msg
    Expect.sequenceEqual result expected "Unexpected parsing result"

let successFile expected filename =
    success expected <| System.IO.File.ReadAllText filename

let failure pattern progString () =
    try parse progString |> failtestf "Success: %O"
    with ParseException(msg) ->
         Expect.isMatch msg pattern "Unexpected error message"

let example1 =
    let n = ENum -13L
    [
        SLabel 1
        SPush <| ENum 1L
        SPush <| ENum 2L; SPush <| ENum 3L

        SPush <| ELabel 1; SJump

        SPush <| ELabel 1; SJump
        SJumpZero
        SPush <| ELabel 1; SJumpZero
        SPush <| ELoad8 (EStack (ENum 3L)); SPush <| ELabel 1; SJumpZero

        SLabel 2
        SPush <| EStack (ENum 0L)
        SPush <| EStack (ENum 1L)

        SPush <| ESum [
            ENum 7L
            EStack (ENum -2L)
            ELoad8 (EStack n)
        ]
        SLabel 3
        SData [0y; 1y; -1y; 1y]
    ]

let assemblyLanguageIntro =
    let prime = ENum 982451653L
    let n = ENum 7L
    let xx = ENum 99L
    let yy = ENeg (ENum 13L)
    let xx2 = ENum 9L
    [
        SLabel 1
        SData [0y; 1y; -2y; -128y; 1y]

        SPush <| ENum 13L
        SPush <| ENum -1L
        SPush <| ENum 0L; SPush <| ENum 1L
        SPush <| ELabel 1
        SPush prime
        SPush <| EStack n
        SPush <| ELoad8 (EStack n)
        SPush <| ESum [ELabel 1; ENeg (ELoad8 (EStack (ENum 0L)))]

        SJump
        SPush <| ELabel 1; SJump
        SJumpZero
        SPush <| ELabel 1; SJumpZero

        SPush <| ESum [prime; ENeg (ELoad8 (EStack (ENum 4L)))]
        SPush <| ELabel 1; SJumpZero
        SJumpNotZero

        SLoad1; SLoad2; SLoad4; SLoad8
        ELabel 1 |> ELoad4 |> ESigx4 |> SPush
        SPush <| ENum -1L

        SStore1; SStore2; SStore4; SStore8
        SPush <| ELabel 1; SStore4
        SPush prime; SPush <| ELabel 1; SStore8

        SAdd
        SPush xx; SAdd

        SPush <| ENum -(99L - 13L); SAdd
        SPush <| ENum -99L; SAdd
        SPush <| ENum (99L + 13L)
        SMult; SNeg
        SDivU; SDivS; SRemU; SRemS

        SAnd
        SPush <| ENum 127L; SAnd
        SPush <| ENum (982451653L &&& 4095L)
        SOr; SXor; SNot;
        SPow2
        SPow2; SMult
        SPow2; SDivU
        SPow2; SDivSU

        SEq
        SPush <| ENum 7L; SEq
        SPush <| ENum 0L // SPush xx; SPush yy; SEq
        SLtU; SLtS; SLtEU; SLtES
        SGtU; SGtS; SGtEU; SGtES

        SAlloc
        SPush prime; SAlloc
        SDealloc
        SPush <| ELoad8 (EStack (ENum 8L)); SDealloc
        SSetSp
        SPush xx2; SSetSp
        SExit
    ]

[<Tests>]
let ExampleFileTests =
    testList "Example files" [
        testCase "Example 1" <| successFile example1 "test_code/example1.s"
        testCase "Intro" <| successFile assemblyLanguageIntro "test_code/assembly_language_intro.s"
    ]

[<Tests>]
let ErrorMessageTests =
    testList "Error messages" [
        testCase "Undefined label" <| failure "^Label not found: xx$" "push! xx"
        // ...
    ]

[<Tests>]
let BasicTests =
    testList "Prog basics" [
        testCase "Offset num" <|
            success
                ([0..3] |> List.map (int64 >> ENum >> EStack >> ELoad8 >> SPush))
                "push!!!! $0 $0 $0 $0"
        testCase "Offset sum" <|
            success
                [
                    SLabel 1
                    ENum 0L |> SPush
                    [ENum 1L; ELabel 1; ELabel 1] |> ESum |> EStack |> SPush
                ]
                "x: push!! 0 &(+ x x)"
    ]
