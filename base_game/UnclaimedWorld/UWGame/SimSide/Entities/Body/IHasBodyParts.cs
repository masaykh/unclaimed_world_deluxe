using System.Collections.Generic;

namespace UWGame.SimSide.Entities.Body;

public interface IHasBodyParts
{
	List<BodyPart> BodyParts { get; set; }
}
