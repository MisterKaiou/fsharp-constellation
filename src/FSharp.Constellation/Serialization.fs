/// Module responsible for handling data serialization.
[<RequireQualifiedAccess>]
module FSharp.Constellation.Serialization

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

/// The default serializer used by CosmosContext instances.
let defaultJsonSerializer =
  let options = JsonSerializerOptions()

  options.Converters.Add(
    JsonFSharpConverter(
      unionEncoding =
        (JsonUnionEncoding.FSharpLuLike
         ||| JsonUnionEncoding.NamedFields)
    )
  )
  options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
  options

/// <summary>Deserializes a stream to <typeparamref name="'a"/>.</summary>
/// <typeparam name="'a">The type to which deserialize to.</typeparam>
/// <returns>The deserialized object.</returns>
let deserialize<'a> (stream: Stream) =
  try
    if typeof<Stream>.IsAssignableFrom typeof<'a> then
      (box stream) :?> 'a
    else
      use memoryStream = new MemoryStream()
      stream.CopyTo(memoryStream)
      let span = ReadOnlySpan(memoryStream.ToArray())
      JsonSerializer.Deserialize(span, options = defaultJsonSerializer)

  finally
    stream.Dispose()

/// <summary>Serializes an <paramref name="input"/> to a Stream.</summary>
/// <param name="input">The input to serialize.</param>
/// <returns>A JSON Stream with the serialized <paramref name="input"/>.</returns>
let serialize input =
  let payload = new MemoryStream()
  let options = JsonWriterOptions(Indented = false)
  use writer = new Utf8JsonWriter(payload, options)

  JsonSerializer.Serialize(writer, input, defaultJsonSerializer)
  payload.Position <- 0
  payload :> Stream
