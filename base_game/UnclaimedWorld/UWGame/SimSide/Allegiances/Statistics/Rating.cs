using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public abstract class Rating : Statistic
{
	public const string RatingsBreakdownTempMessage = "Computing a new rating. (If paused, unpause the game)";

	protected bool composeBreakdown;

	protected string ratingsBreakdown = "Computing a new rating. (If paused, unpause the game)";

	public List<DataPoint<float>> Ratings = new List<DataPoint<float>>();

	protected const int minimumDataPointsToStore = 1;

	private Snapshotter.Version version;

	protected Rating()
	{
	}

	public Rating(double pollInterval)
		: base(pollInterval)
	{
	}

	public override void Update(GameTime gameTime)
	{
		if (Ratings.Count == 0)
		{
			GatherPolledData();
		}
		else
		{
			base.Update(gameTime);
		}
	}

	public override void GatherPolledData()
	{
		float value = ScoreRating();
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		Ratings.Add(new DataPoint<float>
		{
			Time = currentTimeDateYear,
			Value = value
		});
		if (!composeBreakdown)
		{
			ratingsBreakdown = "Computing a new rating. (If paused, unpause the game)";
		}
	}

	protected abstract float ScoreRating();

	public override float GetChange()
	{
		return Statistic.GetChange(Ratings);
	}

	public virtual void AddSharedRating(float ratingValue)
	{
	}

	protected bool IsPlaySite()
	{
		return LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID).GetAllegiance.Site.IsPlaySite;
	}

	public override float GetLatestValue()
	{
		if (Ratings.Count > 0)
		{
			return Ratings.Last().Value;
		}
		return 0f;
	}

	public string GetRatingsBreakdown()
	{
		return ratingsBreakdown;
	}

	public void ToggleComposeBreakdown(bool compose)
	{
		composeBreakdown = compose;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Ratings = sn.DoList(Ratings);
		sn.Ignore(ratingsBreakdown);
		sn.Ignore(composeBreakdown);
		return this;
	}
}
