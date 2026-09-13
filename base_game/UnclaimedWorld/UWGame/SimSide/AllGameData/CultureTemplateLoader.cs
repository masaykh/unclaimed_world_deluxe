using System.Collections.Generic;
using UWGame.SimSide.Entities.Templates;

namespace UWGame.SimSide.AllGameData;

public class CultureTemplateLoader
{
	public static List<CultureTemplate> Init()
	{
		List<CultureTemplate> list = new List<CultureTemplate>();
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantWhiteCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "male",
			RaceKey = "whiteHumanDescendant",
			CommonFirstNames = new string[19]
			{
				"John", "Jon", "Jim", "Walt", "Roger", "Niel", "Andon", "Paul", "Steven", "Samuel",
				"Marek", "Jesper", "Jens", "Red", "Tyson", "Bradley", "Anders", "Mark", "Lau"
			},
			CommonLastNames = new string[24]
			{
				"Green", "Beaumont", "Nyman", "Morozov", "Rence", "Reikhart", "Vaidman", "Robinson", "Roux", "Durand",
				"Welley", "Clark", "Beck", "Cramarr", "Vasilev", "Zahnder", "Gelaesen", "Zholud", "Stefan", "Runar",
				"Vonseiten", "Pennington", "Lloyd", "Hansson"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantAsianCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "male",
			RaceKey = "asianHumanDescendant",
			CommonFirstNames = new string[14]
			{
				"Timur", "Hiroto", "Ren", "Sho", "Michael", "Nick", "John", "Dean", "Razvan", "Chris",
				"Julian", "Coby", "Ike", "Marcus"
			},
			CommonLastNames = new string[7] { "Saito", "Bashi", "Inoue", "Mani", "Weng", "Mellon", "Tuke" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantHispanicCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "male",
			RaceKey = "hispanicHumanDescendant",
			CommonFirstNames = new string[13]
			{
				"Ruben", "Tau", "Nathan", "Amer", "Sebastin", "Matas", "Daniel", "David", "Nicky", "Emanuel",
				"Mike", "Enzo", "Ziki"
			},
			CommonLastNames = new string[7] { "Sica", "Gutierrez", "Manrique", "Vargas", "Manes", "Fernandez", "Jimenez" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantBlackCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "male",
			RaceKey = "blackHumanDescendant",
			CommonFirstNames = new string[14]
			{
				"Kato", "Christian", "John", "Daman", "Will", "Sekan", "Garai", "Robert", "Joseph", "Jonathan",
				"Pyre", "Marc", "Etienne", "Jacob"
			},
			CommonLastNames = new string[9] { "Williams", "Thomas", "Girard", "Marost", "David", "Tiler", "Ka", "Anderson", "Powers" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femaleDescendantWhiteCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "female",
			RaceKey = "whiteHumanDescendant",
			NoOfPortraitFlavours = 2,
			CommonFirstNames = new string[16]
			{
				"Cherea", "Thera", "Tine", "Julia", "Nisa", "Petia", "Chlo", "Mari", "Sabella", "Eres",
				"Lyne", "Elin", "Rena", "Kimby", "Julie", "Jan"
			},
			CommonLastNames = new string[17]
			{
				"Smith", "Vartanian", "Sokolof", "Wilson", "Moore", "Broun", "Harrys", "Kozlov", "Morel", "Katyna",
				"Elitz", "Huse", "Sky", "Hart", "Sullivan", "Zimmer", "van Veen"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femaleDescendantBlackCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 20f,
				Max = 60f
			},
			CasteKey = "female",
			RaceKey = "blackHumanDescendant",
			CommonFirstNames = new string[10] { "Gabrielle", "Jane", "Julia", "Ema", "Linda", "Eva", "Mara", "Jessa", "Patricia", "Valeri" },
			CommonLastNames = new string[10] { "Michel", "Clark", "Laurent", "Isah", "Tores", "Artis", "Fair", "Ganda", "Gauvin", "Richard" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "dogCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 2f,
				Max = 5f
			},
			CommonFirstNames = new string[3] { "Rover", "Max", "Buster" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantWhiteCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "whiteHumanDescendant",
			CommonFirstNames = new string[22]
			{
				"John", "Celoy", "Jim", "Walt", "Cyrus", "Alex", "Paul", "Steven", "Samuel", "Adrian",
				"Costin", "Paul", "Jake", "Jasper", "Kirk", "Leonard", "Miles", "Nat", "Oliver", "Randy",
				"Sid", "Theo"
			},
			CommonLastNames = new string[27]
			{
				"Millet", "Walker", "Perroh", "Brayman", "Langsdor", "Henze", "Nelson", "Goan", "Pierce", "Late",
				"Nord", "Fritt", "Rojartz", "Lavin", "Kaspar", "Novak", "Banik", "Tesar", "Simonis", "Dalca",
				"Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantAsianCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "asianHumanDescendant",
			CommonFirstNames = new string[16]
			{
				"Adi", "Tri", "Raja", "Sang", "Hyun", "Ren", "Sho", "Nick", "John", "Dean",
				"Jim", "Chris", "Coby", "Ike", "Marcus", "Michael"
			},
			CommonLastNames = new string[8] { "Kim", "Lee", "Jiang", "Sato", "Zhou", "Ruan", "Darzi", "Joshi" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantHispanicCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "hispanicHumanDescendant",
			CommonFirstNames = new string[14]
			{
				"Adam", "Luca", "Nathan", "Amer", "Sebastin", "Matas", "Daniel", "David", "Aurel", "Emanuel",
				"Mike", "Enzo", "Sorin", "Victor"
			},
			CommonLastNames = new string[10] { "Villa", "Vargas", "Sastre", "Rana", "Gebara", "Mendoza", "Viteri", "Aritza", "Ferrer", "Roig" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "maleDescendantBlackCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "blackHumanDescendant",
			CommonFirstNames = new string[20]
			{
				"Kofi", "Amos", "Christian", "John", "Akan", "Will", "Sekan", "Akachi", "Robert", "Joseph",
				"Jerome", "Chidi", "Marc", "Kojo", "Jacob", "Isiah", "Jamie", "Udo", "Lewis", "Mack"
			},
			CommonLastNames = new string[10] { "Williams", "Thomas", "Tash", "Stacks", "Howe", "Ivers", "Ka", "Anderson", "Powers", "Laurent" }
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femaleDescendantWhiteCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "female",
			RaceKey = "whiteHumanDescendant",
			NoOfPortraitFlavours = 2,
			CommonFirstNames = new string[35]
			{
				"Corina", "Emilia", "Irina", "Liana", "Lidia", "Luisa", "Mirela", "Mari", "Sabella", "Eres",
				"Lyne", "Elin", "Rena", "Kimby", "Julie", "Jan", "Edina", "Dara", "Sophea", "Chea",
				"Chan", "Stela", "Magda", "Miruna", "Ramona", "Alise", "Christie", "Emmie", "Karin", "Kirsten",
				"Laura", "Leona", "Paula", "Tara", "Vera"
			},
			CommonLastNames = new string[19]
			{
				"Pierce", "Late", "Nord", "Fritt", "Rojartz", "Lavin", "Kaspar", "Novak", "Banik", "Tesar",
				"Simonis", "Dalca", "Vasile", "Foss", "Giles", "Mullins", "Travere", "Viteri", "Aritza"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femaleDescendantBlackCulture2",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "female",
			RaceKey = "blackHumanDescendant",
			CommonFirstNames = new string[36]
			{
				"Sarah", "Taylis", "Julia", "Ana", "Linda", "Eva", "Mara", "Sanda", "Kiri", "Valeri",
				"Amara", "Adanna", "Liza", "Alea", "Alyx", "Becca", "Brook", "Callista", "Cassie", "Christa",
				"Cora", "Dani", "Elea", "Elise", "Gail", "Erika", "Gemma", "Gillian", "Helena", "Ina",
				"Janey", "Jeannie", "Kelly", "Lana", "Reene", "Serena"
			},
			CommonLastNames = new string[12]
			{
				"Michel", "Clark", "Laurent", "Artis", "Fair", "Wade", "Wray", "Rains", "Reier", "Sadler",
				"Allard", "Tash"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallWhiteAngloCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "whiteHumanAncestor",
			CommonFirstNames = new string[39]
			{
				"Thomas", "John", "Jim", "Walt", "Alex", "Axel", "Andrew", "Aaron", "Paul", "Steven",
				"Paul", "Jake", "Jasper", "Christopher", "Chris", "Carl", "Case", "Clay", "Jay", "Jeremy",
				"Rick", "Stevie", "Dave", "Douglas", "Edward", "Eric", "Frank", "Rich", "Robin", "Ronny",
				"Scott", "Shane", "Simon", "Ted", "Matthew", "Charlie", "Noah", "Jack", "James"
			},
			UncommonFirstNames = new string[73]
			{
				"Arthur", "Cyrus", "Avery", "Austin", "Samuel", "Adrian", "Theo", "Kirk", "Leonard", "Miles",
				"Oliver", "Randy", "Sid", "Brendan", "Bruce", "Buzz", "Carter", "Conrad", "Heath", "Morgan",
				"Reynold", "Rodney", "Timothy", "Darrell", "Garrett", "Gerard", "Glenn", "Grant", "Greg", "Hal",
				"Harvey", "Henry", "Hugh", "Hugo", "Irving", "Jeff", "Jeffrey", "Jeremiah", "Jesse", "Jimmy",
				"Joel", "Julian", "Keith", "Kenneth", "Kiefer", "Kyle", "Lance", "Lawrence", "Lou", "Luke",
				"Matt", "Max", "Milo", "Mitch", "Nat", "Nate", "Nash", "Neil", "Niles", "Oliver",
				"Owen", "Philip", "Quinn", "Ralph", "Randall", "Raymond", "Reggie", "Reuben", "Emmett", "Timmy",
				"Victor", "Walter", "Wilson"
			},
			CommonLastNames = new string[38]
			{
				"Walker", "Brayman", "Nelson", "Nielsen", "Goan", "Pierce", "Late", "Fritt", "Lavin", "Kaspar",
				"Novak", "Beaumont", "Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss", "Smith",
				"Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Thomas", "Moore", "Martin",
				"Jackson", "Thompson", "White", "Lee", "Harris", "Clark", "Lewis", "Robinson"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallWhiteRussianCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "whiteHumanAncestor",
			CommonFirstNames = new string[34]
			{
				"Piotr", "Alexander", "Maxim", "Ivan", "Artyom", "Dmitry", "Mikhail", "Nikita", "Daniil", "Yegor",
				"Andrei", "Viktor", "Boris", "Dmitry", "Pavel", "Ruslan", "Feliks", "Denis", "Gennadi", "Isaak",
				"Kazimir", "Kiril", "Konstantin", "Lazar", "Maxim", "Sergey", "Stephan", "Timur", "Vadim", "Vlad",
				"Vladimir", "Yanick", "Yevgeni", "Zakhar"
			},
			CommonLastNames = new string[18]
			{
				"Smirnov", "Ivanov", "Kuznetsov", "Popov", "Sokolov", "Lebedev", "Kozlov", "Novikov", "Morozov", "Petrov",
				"Volkov", "Vasilyev", "Zaytsev", "Yazova", "Zaytsev", "Timmerman", "Stasov", "Tikhonov"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallChineseCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "asianHumanAncestor",
			CommonFirstNames = new string[6] { "Wei", "Hao", "Dong", "Ming", "Tao", "Peng" },
			CommonLastNames = new string[17]
			{
				"Li", "Wang", "Zhang", "Liu", "Chen", "Yang", "Huang", "Zhao", "Zhou", "Wu",
				"Xu", "Sun", "Zhu", "Ma", "Hu", "Guo", "Lin"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallJapaneseCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "asianHumanAncestor",
			CommonFirstNames = new string[9] { "Hiroto", "Shota", "Ren", "Sota", "Sora", "Yuto", "Yuma", "Eita", "Sho" },
			CommonLastNames = new string[12]
			{
				"Sato", "Tanaka", "Ito", "Saito", "Kato", "Yoshida", "Yamada", "Sasaki", "Inoue", "Kimura",
				"Hayashi", "Shimizu"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallHispanicCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "hispanicHumanAncestor",
			CommonFirstNames = new string[39]
			{
				"Adrian", "Agustin", "Alejandro", "Alonso", "Alvaro", "Angel", "Antonio", "Benjamin", "Bruno", "Carlos",
				"Cesar", "Daniel", "David", "Diego", "Francisco", "Gabriel", "Hugo", "Iker", "Javier", "Jeronimo",
				"Joaquin", "Jose", "Juan", "Luis", "Mario", "Martin", "Mateo", "Matias", "Miguel", "Nicolas",
				"Pablo", "Pedro", "Ramon", "Rodrigo", "Samuel", "Santiago", "Sebastian", "Tomas", "Vicente"
			},
			UncommonFirstNames = new string[1] { "Thiago" },
			CommonLastNames = new string[31]
			{
				"Rocha", "Villa", "Vargas", "Sastre", "Rana", "Gebara", "Mendoza", "Viteri", "Aritza", "Ferrer",
				"Roig", "Gonzalez", "Hernandez", "Ramirez", "Rodriguez", "Gutierrez", "Ortiz", "Morales", "Sanchez", "Martinez",
				"Lopes", "Dias", "Gomez", "Torres", "Flores", "Garcia", "Ruiz", "Velazques", "Jimenez", "Cruz",
				"Iglesia"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "malePlanetfallAfricanCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "male",
			RaceKey = "blackHumanAncestor",
			CommonFirstNames = new string[24]
			{
				"Kofi", "Christian", "John", "Akan", "Will", "Kojo", "Akachi", "Robert", "Joseph", "Jerome",
				"Chidi", "Marc", "Kojo", "Jacob", "Mack", "Basile", "Bertrand", "David", "Didier", "Jean",
				"Leon", "Marcel", "Max", "Nicolas"
			},
			CommonLastNames = new string[25]
			{
				"Kwena", "Williams", "Thomas", "Oni", "Eze", "Martins", "Okafor", "Savage", "Lawal", "Laurent",
				"Abiola", "Adebisi", "Layeni", "Taiwo", "Oyekan", "Balewa", "Iwu", "Sekibo", "Madaki", "Akerele",
				"Igwe", "Ohakim", "Jakande", "Chike", "Yeboah"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femalePlanetfallWhiteAngloCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "female",
			RaceKey = "whiteHumanAncestor",
			NoOfPortraitFlavours = 2,
			CommonFirstNames = new string[44]
			{
				"Julie", "Dara", "Ramona", "Christie", "Kirsten", "Laura", "Leona", "Paula", "Mary", "Patricia",
				"Linda", "Barbara", "Elizabeth", "Jennifer", "Maria", "Susan", "Sophia", "Emily", "Isabella", "Madison",
				"Olivia", "Emma", "Ava", "Hailey", "Abigail", "Kaitlyn", "Mia", "Ruby", "Evie", "Lily",
				"Charlotte", "Mia", "Ava", "Emily", "Sofia", "Sophie", "Chloe", "Ella", "Isla", "Amelia",
				"Isabella", "Grace", "Lucy", "Jessica"
			},
			UncommonFirstNames = new string[6] { "Alise", "Emmie", "Karin", "Tara", "Vera", "Harper" },
			CommonLastNames = new string[38]
			{
				"Walker", "Brayman", "Nelson", "Nielsen", "Goan", "Pierce", "Late", "Fritt", "Lavin", "Kaspar",
				"Novak", "Beaumont", "Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss", "Smith",
				"Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Thomas", "Moore", "Martin",
				"Jackson", "Thompson", "White", "Lee", "Harris", "Clark", "Lewis", "Robinson"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femalePlanetfallRussianCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "female",
			RaceKey = "whiteHumanAncestor",
			NoOfPortraitFlavours = 2,
			CommonFirstNames = new string[27]
			{
				"Irina", "Anastasia", "Mariya", "Dariya", "Anna", "Polina", "Viktoria", "Yekaterina", "Sofia", "Alexandra",
				"Irina", "Kira", "Lana", "Lidiya", "Manya", "Ludmila", "Melana", "Nadya", "Natasha", "Oxana",
				"Sacha", "Sabina", "Sonya", "Tania", "Valentina", "\tYaneta", "Zhanna"
			},
			CommonLastNames = new string[18]
			{
				"Nadova", "Smirnova", "Ivanova", "Kuznetsova", "Popova", "Sokolova", "Lebedeva", "Kozlova", "Novikova", "Morozova",
				"Petrova", "Volkova", "Zaytseva", "Yazova", "Zaytseva", "Timmerman", "Stasova", "Tikhonova"
			}
		});
		list.Add(new CultureTemplate
		{
			KeyName = "femalePlanetfallAfricanCulture",
			AgeInYears = new NormalDistribution
			{
				Min = 18f,
				Max = 68f
			},
			CasteKey = "female",
			RaceKey = "blackHumanAncestor",
			CommonFirstNames = new string[33]
			{
				"Augustine", "Sarah", "Ama", "Ayo", "Ezewa", "Daraya", "Kisha", "Kenia", "Kabiite", "Malene",
				"Nadira", "Safika", "Sisi", "Tana", "Wamani", "Zakia", "Nkechi", "Nuru", "Julia", "Linda",
				"Eva", "Mara", "Sanda", "Kiri", "Amara", "Adanna", "Liza", "Lana", "Serena", "Zina",
				"Yana", "Zuwena", "Tekene"
			},
			CommonLastNames = new string[29]
			{
				"Kwena", "Michel", "Clark", "Laurent", "Williams", "Thomas", "Oni", "Eze", "Martins", "Okafor",
				"Savage", "Lawal", "Laurent", "Abiola", "Adebisi", "Layeni", "Taiwo", "Oyekan", "Balewa", "Iwu",
				"Sekibo", "Madaki", "Akerele", "Igwe", "Ohakim", "Jakande", "Chike", "Allard", "Yeboah"
			}
		});
		return list;
	}
}
