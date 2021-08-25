using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Audience
{
    //private const float REACTION_PROBABILITY = 0.75f;

    public enum ReactionTypes: byte
    {
         MinorApplause,MajorApplause,
         MinorDisappointment,MajorDisappointment,
         PositiveSurprise,NeutralSurprise,NegativeSurprise,
         Laughter,
    }

public static void React(ReactionTypes reactionType)
    {
        SoundNames reactionSoundName = SoundNames.None;
        switch (reactionType)
        {
            case ReactionTypes.MinorApplause:
                {
                    reactionSoundName = SoundNames.AudienceMinorApplause;
                }
                break;
            case ReactionTypes.MinorDisappointment:
                {
                    reactionSoundName = SoundNames.AudienceMinorDisappointment;
                }
                break;
            case ReactionTypes.PositiveSurprise:
                {
                    reactionSoundName = SoundNames.AudiencePositiveSurprise;
                }
                break;
            case ReactionTypes.NeutralSurprise:
                {
                    reactionSoundName = SoundNames.AudienceNeutralSurprise;
                }
                break;
            case ReactionTypes.NegativeSurprise:
                {
                    reactionSoundName = SoundNames.AudienceNegativeSurprise;
                }
                break;
            case ReactionTypes.Laughter:
                {
                    reactionSoundName = SoundNames.AudienceLaugh;
                }
                break;


        }
        SoundManager.PlayOneShotSound(reactionSoundName, null);
    }

    /*private static bool ShouldReact()
    {
        float random = Random.Range(0f, 1f);
        Debug.Log("random:" + random);
        return (random < REACTION_PROBABILITY);
    }*/
}
