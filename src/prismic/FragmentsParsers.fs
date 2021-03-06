﻿namespace prismic
open System
open FSharp.Data
open FSharp.Data.JsonExtensions
open Shortcuts
open Fragments

module internal FragmentsParsers =

    let parseWebLink (f:JsonValue) =
        WebLink({
                url = f?url.AsString();
                contentType = Option.None
        })
    let parseFragmentWebLink (f:JsonValue) = Fragment.Link(parseWebLink(f))
    let parseDocumentLink (f:JsonValue) =
        let doc = f?document in
        let isBroken = (asBooleanOption(f>?"isBroken")) <?- false
        DocumentLink({
                        id = doc?id.AsString();
                        typ = doc.GetProperty("type").AsString();
                        tags = asArrayOrEmpty doc "tags" |> Array.map (fun j -> j.AsString()) |> Seq.ofArray;
                        slug = doc?slug.AsString();
                        isBroken = isBroken})
    let parseFragmentDocumentLink (f:JsonValue) = Fragment.Link(parseDocumentLink(f))
    let parseMediaLink (f:JsonValue) =
        let file = f?file in
        MediaLink({
                    url = file?url.AsString();
                    kind = file?kind.AsString();
                    size = file?size.AsInteger64();
                    filename = file?name.AsString()})
    let parseFragmentMediaLink (f:JsonValue) = Fragment.Link(parseMediaLink(f))
    let parseImageLink (f:JsonValue) =
        let image = f?image in
        ImageLink({
                    name = image?name.AsString();
                    url = image?url.AsString();
                    size = image?size.AsInteger64();
                    height = image?height.AsInteger64();
                    width = image?width.AsInteger64()})
    let parseFragmentImageLink (f:JsonValue) = Fragment.Link(parseImageLink(f))
    let parseImageView (f:JsonValue) = {
        url=  f?url.AsString();
        width= f?dimensions?width.AsInteger();
        height= f?dimensions?height.AsInteger();
        alt= asStringOption(f>?"alt");
        linkTo= match f.TryGetProperty("linkTo") with
                | Some(json) -> match json.TryGetProperty("type") |> Option.map (fun t -> t.AsString()) with
                                | Some("Link.web") -> Some(parseWebLink json?value)
                                | Some("Link.document") -> Some(parseDocumentLink json?value)
                                | Some("Link.file") -> Some(parseMediaLink json?value)
                                | Some("Link.image") -> Some(parseImageLink json?value)
                                | _ -> None
                | _ -> None
    }
    let parseImage (f:JsonValue) =
                        let main = parseImageView f?main
                        let views = asTupleListFromProperties parseImageView f?views
                        Image(main, views)

    let parseColor (f:JsonValue) =
        let s = f.AsString()
        let parsed = tryParseHexColor(s) <?-- (lazy raise (ArgumentException("Invalid color value " + s)))
        Color({hex=s})
    let parseNumber (f:JsonValue) =
        Number(
            f.AsFloat()) // err : Invalid number value
    let parseDate (f:JsonValue) =
        Date(
            f.AsDateTime()) // err : Invalid date value
    let parseTimestamp (f:JsonValue) =
        Timestamp(
            f.AsDateTime()) // err : Invalid date value
    let parseUnixMsDate (f:JsonValue) =
        Date(
            asDateTimeFromUnixMs f) // err : Invalid date value
    let parseText (f:JsonValue) =
        Text(
            f.AsString()) // err : Invalid text value
    let parseSelect (f:JsonValue) =
        Text(
            f.AsString()) // err : Invalid text value
    let parseEmbed (f:JsonValue) =
        let oembed = f?oembed in
        {
            typ = oembed.GetProperty("type").AsString();
            provider = asStringOption(oembed>?"provider_name");
            url = oembed?embed_url.AsString();
            width = asIntegerOption(oembed>?"width");
            height = asIntegerOption(oembed>?"height");
            html = asStringOption(oembed>?"html");
            oembedJson = oembed
            label = asStringOption(oembed>?"label");
        }
    let parseFragmentEmbed (f:JsonValue) = Fragment.Embed(parseEmbed(f))
    let parseGeoPoint (f:JsonValue) =
        GeoPoint({
                 latitude = asDecimal(f?latitude);
                 longitude = asDecimal(f?longitude)
        })

    let parseStructuredText (f:JsonValue) =
        let parseSpan (f:JsonValue) =
            let type' = f.GetProperty("type").AsString()
            let start = f?start.AsInteger()
            let end' = f.GetProperty("end").AsInteger()
            let data = f>?"data"
            match (type', data) with
                        | ("strong", _) -> Strong(start, end')
                        | ("em", _) -> Em(start, end')
                        | ("label", Some(d)) -> Label(start, end', d?label.AsString())
                        | ("hyperlink", Some(d)) when d.GetProperty("type").AsString() = "Link.web" ->
                            Hyperlink(start, end', parseWebLink(d?value))
                        | ("hyperlink", Some(d)) when d.GetProperty("type").AsString() = "Link.document" ->
                            Hyperlink(start, end', parseDocumentLink(d?value))
                        | ("hyperlink", Some(d)) when d.GetProperty("type").AsString() = "Link.file" ->
                            Hyperlink(start, end', parseMediaLink(d?value))
                        | ("hyperlink", Some(d)) when d.GetProperty("type").AsString() = "Link.image" ->
                            Hyperlink(start, end', parseImageLink(d?value))
                        | (t, _) -> raise (ArgumentException("Unsupported span type "+ t))
        let parseSpanSeq (f:JsonValue) = f?spans.AsArray() |> Array.map (fun s -> parseSpan s) |> Seq.ofArray
        let parseHeading level (f:JsonValue) =
            Block.Text(
                Heading(
                        f?text.AsString(),
                        parseSpanSeq f,
                        level,
                        asStringOption(f>?"label")
                        ))
        let parseParagraph (f:JsonValue) =
            Block.Text(
                Paragraph(
                    f?text.AsString(),
                    parseSpanSeq f,
                    asStringOption(f>?"label")
                    ))
        let parsePreformatted (f:JsonValue) =
            Block.Text(
                Preformatted(
                    f?text.AsString(),
                    parseSpanSeq f,
                    asStringOption(f>?"label")
                    ))
        let parseListItemWithOrdered ordered (f:JsonValue) =
            Block.Text(
                ListItem(
                    f?text.AsString(),
                    parseSpanSeq f,
                    ordered,
                    asStringOption(f>?"label")
                    ))
        let parseListItem = parseListItemWithOrdered false
        let parseOListItem = parseListItemWithOrdered true
        let parseBlockImage (f:JsonValue) = Block.Image(parseImageView f)
        let parseBlockEmbed (f:JsonValue) = Block.Embed(parseEmbed f)

        let parseBlock (f:JsonValue) = match f.GetProperty("type").AsString() with
                                        | "heading1" -> parseHeading 1 f
                                        | "heading2" -> parseHeading 2 f
                                        | "heading3" -> parseHeading 3 f
                                        | "heading4" -> parseHeading 4 f
                                        | "heading5" -> parseHeading 5 f
                                        | "heading6" -> parseHeading 6 f
                                        | "paragraph" -> parseParagraph f
                                        | "preformatted" -> parsePreformatted f
                                        | "list-item" -> parseListItem f
                                        | "o-list-item" -> parseOListItem f
                                        | "image" -> parseBlockImage f
                                        | "embed" -> parseBlockEmbed f
                                        | t -> raise (ArgumentException("Unsupported block type " + t))
        let blocks = f.AsArray() |> Array.map (fun a -> parseBlock a) |> Seq.ofArray
        StructuredText(blocks)


    let rec parseFragment (j:JsonValue) =
        let parseGroup (f:JsonValue) =
            let g = f.AsArray() |> Array.map (fun x -> {fragments = asTupleListFromOptionProperties parseFragment x}) |> Seq.ofArray
            Group(g)

        let t = j.GetProperty("type").AsString() in
        let maybeParser = match t with
                            | "Image" -> Some(parseImage)
                            | "Color" -> Some(parseColor)
                            | "Number" -> Some(parseNumber)
                            | "Date" -> Some(parseDate)
                            | "Timestamp" -> Some(parseTimestamp)
                            | "Text" -> Some(parseText)
                            | "Select" -> Some(parseSelect)
                            | "Embed" -> Some(parseFragmentEmbed)
                            | "GeoPoint" -> Some(parseGeoPoint)
                            | "Link.web" -> Some(parseFragmentWebLink)
                            | "Link.document" -> Some(parseFragmentDocumentLink)
                            | "Link.file" -> Some(parseFragmentMediaLink)
                            | "Link.image" -> Some(parseFragmentImageLink)
                            | "StructuredText" -> Some(parseStructuredText)
                            | "Group" -> Some(parseGroup)
                            | _ -> None
        let value = j.GetProperty("value") in
        maybeParser |> Option.bind (fun parser -> Some(parser value))

    type ParsedFieldArrayElement = (int * Fragment)
    type ParsedField = Single of Fragment
                        | Array of ParsedFieldArrayElement[]

    let parseField (j:JsonValue) =
        match j with
            | JsonArray a ->
                            let elements = a
                                            |> Array.mapi (fun i o -> match parseFragment o with
                                                                                    | Some(f) -> Some(i, f)
                                                                                    | _ -> None)
                                            |> Array.choose id
                            Some(Array(elements))
            | o -> match parseFragment o with
                            | Some(f) -> Some(Single(f))
                            | _ -> None
