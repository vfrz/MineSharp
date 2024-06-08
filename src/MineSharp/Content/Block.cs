using MineSharp.Sdk.Core;

namespace MineSharp.Content;

public record struct Block(BlockId BlockId, byte Metadata, byte Light, byte Skylight);