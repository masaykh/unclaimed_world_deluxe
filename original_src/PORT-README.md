# The studio's released source

This is **Refactored Games' own source for Unclaimed World**, exactly as they released it at
[spunky44/UnclaimedWorld](https://github.com/spunky44/UnclaimedWorld), carried here unmodified.
Their `README.md` and `LICENSE.md` are beside this file.

**Nothing here is built.** Not by the solution, not by CI, not by any script in `tools/build/`.
It is reference material, and the two `Directory.*.props` files next to this one exist only to
stop MSBuild and NuGet inheriting the repository's real build settings into it.

## Why it is in the repository

`base_game/` is **decompiled** from the shipped 1.0.4.8 assembly, and comments do not survive
into IL. No decompiler can bring them back, so they were never in `base_game/` to begin with -
not removed, never present.

The difference is not small. This tree carries **68,420 comment lines** that `base_game/` does
not, and a large share of them are commented-out code: the unfinished features, the abandoned
branches, and the studio's own notes about why something is the way it is.
`Sim/PlaceGameEntities.cs` is the clearest case - 14,666 lines here against 3,812 there, with
1,730 lines of commented-out code that simply have no counterpart in the decompiled file.

That material is the reason people go looking for the original source, and until now they had to
go to GitHub to find it while reading a tree that could not explain itself.

## What is here and what is not

Source text only: `.cs`, `.csproj`, `.sln`, `.config`, plus their README and LICENSE. No `bin/`,
no `obj/`, no `Content/` - the full tree is 4.1 GB and almost all of it is build output and
compiled assets. The studio's **asset sources** are already in `assets/`, and their compiled
`Content/` ships in the release archives, so neither belongs here twice.

## Its relationship to `base_game/`

Refactored Games released the source at **1.0.4.7**; Steam ships **1.0.4.8**. `tools/SourceCompare`
compared them: building 1.0.4.7 and decompiling it with the same `ilspycmd` settings gives
**1801 types and 25,974 members on both sides, with zero differences**. Method bodies cannot be
compared exactly - 1.0.4.7 only rebuilds under a modern Roslyn while 1.0.4.8 came from a C# 6-era
compiler, and that perturbs every body-level measurement - so the honest statement is that no
functional difference was found and the measurement cannot prove there is none. See
`PORTING-NOTES.md` in the porting repository for the full working.

So this is not a second version of the game. It is the same game, with its comments intact.

## The rebase this exists for

The agreed architecture is that `base_game/` should **be** this source with the port's core
changes applied on top, rather than decompiler output with changes patched into it. That work is
pending: the delta is 51 files and roughly 3,400 lines, and the changes cannot be applied as text
patches - the two trees are structurally identical and textually very different, so each one is
re-applied by meaning.

Some changes disappear on the way. MonoGame added `MathHelper.Max(int, int)` overloads XNA never
had, so the decompiler resolved them and dropped casts the studio actually wrote; the original is
simply correct there and the port's `(int)` casts are decompiler damage.

Until that lands, read this tree when you want to know **why** - and `base_game/` when you want to
know what actually runs.
