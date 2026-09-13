using System.Collections.Generic;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class PersonalityLoader
{
	public static List<PersonalityType> Init()
	{
		return new List<PersonalityType>
		{
			new PersonalityType("earlyJoinerPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.550000011920929,
						StandardDeviation = 0.10000000149011612
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.20000000298023224,
							StandardDeviation = 0.05000000074505806
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.15000000596046448,
							StandardDeviation = 0.05000000074505806
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.10000000149011612,
							StandardDeviation = 0.07999999821186066
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			},
			new PersonalityType("survivalTierPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.30000001192092896,
						StandardDeviation = 0.20000000298023224
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.20000000298023224,
							StandardDeviation = 0.05000000074505806
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.15000000596046448,
							StandardDeviation = 0.05000000074505806
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.10000000149011612,
							StandardDeviation = 0.07999999821186066
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			},
			new PersonalityType("basicTierPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.20000000298023224,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.3499999940395355,
							StandardDeviation = 0.15000000596046448
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.30000001192092896,
							StandardDeviation = 0.10000000149011612
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.30000001192092896,
							StandardDeviation = 0.10000000149011612
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			},
			new PersonalityType("mediumTierPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.10000000149011612,
						StandardDeviation = 0.03999999910593033
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.5,
							StandardDeviation = 0.15000000596046448
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.550000011920929,
							StandardDeviation = 0.12999999523162842
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.550000011920929,
							StandardDeviation = 0.12999999523162842
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			},
			new PersonalityType("randomTierPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.15000000596046448,
						StandardDeviation = 0.18000000715255737
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.6000000238418579,
							StandardDeviation = 0.4000000059604645
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.5,
							StandardDeviation = 0.5
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.5,
							StandardDeviation = 0.5
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			},
			new PersonalityType("advancedSecurityPersonality")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.15000000596046448,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.3499999940395355,
							StandardDeviation = 0.15000000596046448
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.800000011920929,
							StandardDeviation = 0.10000000149011612
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.30000001192092896,
							StandardDeviation = 0.10000000149011612
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>()
			}
		};
	}
}
