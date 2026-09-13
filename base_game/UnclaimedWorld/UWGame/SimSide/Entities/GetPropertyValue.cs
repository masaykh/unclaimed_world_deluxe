using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Entities;

public delegate PropertyResult? GetPropertyValue(IHasExposedProperties presentedObject, SharedKnowledge getterKnowledge, IHasExposedProperties parent);
