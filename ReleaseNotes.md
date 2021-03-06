### New in 1.0.0 (Released 2014/05/30)
* Initial version
* API for providing binding to the prismic.io REST API.
* Extensions for C#.

### New in 1.0.1 (Released 2014/05/30)
* New extensions for C#.

### New in 1.0.2 (Released 2014/06/01)
* New builder for DocumentLinkResolver
* Renamed ref to refId, more convenient not to deal with reserved keywords
* Changed a module name (Infra to ApiInfra)
* bug fix : ScheduledAt date parsed from unix time
* Use a Tuple List instead of a Map for storing documents in order to maintain order

### New in 1.0.3 (Released 2014/06/06)
* Set exact version number of FSharpData

### New in 1.0.4 (Released 2014/07/04)
* Add a release Id on Ref
* Add Linked Documents on Document
* Add a simple cache

### New in 1.0.5 (Released 2014/08/22)
* Add a Embed getter
* Add GeoPoint support

### New in 1.0.6 (Released 2014/08/25)
* Add C# extension for GeoPoint & Embed fragment.

### New in 1.0.7 (Released 2014/09/23)
* Add support for Timestamp fragments.

### New in 1.0.8 (Release 2014/09/26)
* Add support for Links in images
* Add support for span and group labels

### New in 1.0.10 (Release 2014/09/30)
* Fix parsing for Links in images

### New in 1.0.11 (Release 2014/10/01)
* Add support for h5 and h6

### New in 1.1.0 (Release 2014/10/02)
* BREAKING CHANGE: In F#, the signature of asHtml and related functions have changed as we introduced the HtmlSerializer. If you don't need to generate custom HTML, just pass HtmlSerializer.Empty. The signature in C# should be unchanged.
* Add support for HtmlSerializer, to control the generated HTML from asHtml
 
#### New in 1.1.1 (Release 2014/11/17)
* OEmbed: make provider optional

