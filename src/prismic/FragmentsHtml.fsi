﻿namespace prismic
open System

/// Signature file for the FragmentsHtml module
module FragmentsHtml =

    val asHtml : linkResolver:(Fragments.DocumentLink->string) -> htmlSerializer:(Fragments.Element->string->string option) -> fragment:Fragments.Fragment -> string

    val imageViewAsHtml : linkResolver:(Fragments.DocumentLink->string) -> imageView:Fragments.ImageView -> string
