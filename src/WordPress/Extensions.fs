namespace WordPress

module Async =
  /// Convenience function to provide `map` functionality to `Async<'a>`
  let map f op =
    async {
      let! x = op
      let value = f x
      return value
    }
