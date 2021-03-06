﻿namespace prismic
open FSharp.Data
open FSharp.Data.JsonExtensions
open System
open Shortcuts

module Fragments =

    /// fragments types

    type ImageView = { url: string; width: int; height: int; alt: string option; linkTo: Link option }
                        member x.ImageRatio = x.width / x.height
    and DocumentLink = { id: string; typ: string; tags: string seq; slug: string; isBroken: bool }
    and WebLink = { url:string; contentType:string option}
    and MediaLink = { url:string; kind:string; size:Int64; filename:string }
    and ImageLink = { name:string; url:string; size:Int64; height:Int64; width:Int64 }
    and GroupDoc = { fragments: TupleList<string, Fragment> }
    and Link = WebLink of WebLink
                | MediaLink of MediaLink
                | DocumentLink of DocumentLink
                | ImageLink of ImageLink
    and Span = Em of (int * int)
                | Strong of (int * int)
                | Hyperlink of (int * int * Link)
                | Label of (int * int * string) // start, end, label
    and Embed = {typ:string; provider:string option; url:string; width:int option; height:int option; html:string option; oembedJson:JsonValue; label: string option}
    and GeoPoint = {latitude: decimal; longitude: decimal}
    and Text = Heading of (string * Span seq * int * string option) // (text, spans, level, label)
                | Paragraph of (string * Span seq * string option) // (text, spans, label)
                | Preformatted of (string * Span seq * string option) // (text, spans, label)
                | ListItem  of (string * Span seq * bool * string option)  // (text, spans, ordered, label)
    and Block = Text of Text
                | Image of ImageView
                | Embed of Embed
    and Element = Span of Span
                | Block of Block
    and Color = { hex:string }
                    member x.asRGB = match tryParseHexColor x.hex with
                                        Some(r :: g :: b :: []) -> (System.Int16.Parse(r), System.Int16.Parse(g), System.Int16.Parse(b))
                                        | _ -> (0s, 0s, 0s)
    and Fragment =  | Link of Link
                    | Text of string
                    | Date of DateTime
                    | Timestamp of DateTime
                    | Number of float // or double ?
                    | Color of Color
                    | Embed of Embed
                    | GeoPoint of GeoPoint
                    | Image of (ImageView * TupleList<string, ImageView>) // (main * views)
                    | Group of GroupDoc seq
                    | StructuredText of Block seq
