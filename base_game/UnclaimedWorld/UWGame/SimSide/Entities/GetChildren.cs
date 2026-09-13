using System.Collections.Generic;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.Entities;

public delegate void GetChildren(IHasExposedProperties presentedObject, PropertyCondition filter, ref List<IHasExposedProperties> result);
