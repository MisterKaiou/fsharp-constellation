namespace Constellation.Tests

module CustomBuilders =

  type CheckerBuilder() =
    member inline _.Yield _ = true

    member inline _.Run(last) = last

    member inline _.Bind(m: bool, f) =
      m
      |> function
        | true -> f m
        | false -> m

    [<CustomOperation("equal")>]
    member inline this.Equal(c, l, r) = this.Bind(c, (fun _ -> l = r))

    [<CustomOperation("isTrue")>]
    member inline this.IsTrue(c, r) = this.Bind(c, (fun _ -> r))

    [<CustomOperation("notEqual")>]
    member inline this.NotEqual(c, l, r) = this.Bind(c, (fun _ -> not (l = r)))

  let check = CheckerBuilder()
