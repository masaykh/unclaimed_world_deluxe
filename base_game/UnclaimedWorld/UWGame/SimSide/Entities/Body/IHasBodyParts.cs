using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Body;

/// <summary>
/// Enables body-part tree operations on both <see cref="Body"/> and <see cref="BodyPart"/>.
/// </summary>
public interface IHasBodyParts
{
	List<BodyPart> BodyParts { get; set; }
}
