# Licensing

Two licenses apply here, to different things.

| what | license | file |
|---|---|---|
| the game — the studio's source, data, scenarios, strings | **Unclaimed World Community License v1.0** | [LICENSE-UnclaimedWorld-Community.md](LICENSE-UnclaimedWorld-Community.md) |
| the port and the tooling — everything written for this project | **MIT** | [LICENSE-port-MIT.txt](LICENSE-port-MIT.txt) |

Third-party libraries keep their own licenses, which neither of the above supersedes — the
Community License says so explicitly (§7). MonoGame, HarmonyLib, Steamworks.NET and the shader
toolchain are each under their own terms.

## What the Community License requires of this project

Refactored Games released the game's source and assets under a license that permits exactly what
this repository does. The obligations that come with it are short, and all of them are met here:

**It must say it is unofficial (§4).** It does, at the top of the README, and it is repeated in
the repository description. *This is an unofficial community project. It is not endorsed by,
affiliated with, or supported by Refactored Games.* Please do not take problems with this build
to them.

**It must be non-commercial (§2).** No sale, no paid access, no advertising, subscriptions,
donations, crowdfunding, Patreon or sponsorship. If you fork this, that travels with it.

**The name is allowed (§3).** The license names *"Unclaimed World Deluxe"* as a permitted form,
alongside *"Unclaimed World Moddable"* and *"Unclaimed World Community"*. What is not allowed is
a name implying an official sequel — *"Unclaimed World 2"* and the like — or any use of the name
or logo suggesting the studio endorses or maintains this.

**Copyright notices stay (§5).** Unclaimed World, its code, art, audio, characters, trademarks
and logos remain the property of Refactored Games (Morten Pedersen).

**Redistribution carries the license (§8).** Any build or fork you distribute must include a copy
of the Community License and identify itself as an unofficial community project.

## What is in this repository, and in a release

**The repository** carries the studio's released source — unmodified, in `original_src/`, under
§1 — and their asset **sources** — 3343 textures, 247 models, 235 sounds, the music, the fonts,
the shaders — plus the maps, the string table, the port's own code, the build tooling and the
mods. It carries no compiled content: no
`.xnb`, because that is a build output.

**A release archive** carries the compiled `Content/` as well, so it runs on its own.

That second point is the one worth being able to defend, so here is the working. Every one of the
549 compiled assets the game ships was checked against
[spunky44/UnclaimedWorld](https://github.com/spunky44/UnclaimedWorld): all 512 distinct basenames
have a released source, counting `WindowSystem/Content/` as well as `UnclaimedWorld/Content/`.
So the compiled `Content/` is a **build of Original Materials**, and §1 grants the right to
"distribute the Original Materials and your modifications".

Rebuilding it from those sources rather than shipping the studio's own build would be cleaner
provenance, and is not possible yet: 47 of the models are ASCII FBX 6.1, which no version of
Assimp reads. See `assets/README.md`.

## If you are packaging a build

Include, in the archive:

- `LICENSE-UnclaimedWorld-Community.md`
- `LICENSE-port-MIT.txt`
- the third-party notices for whatever natives you shipped
- a visible statement that the build is unofficial

`tools/build/60-package-gl.sh` is where to add anything missing from that list.
