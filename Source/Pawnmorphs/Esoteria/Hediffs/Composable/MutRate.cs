﻿using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines how quickly mutations are gained
    /// </summary>
    public abstract class MutRate
    {
        /// <summary>
        /// How many mutations to queue up for the next second.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public virtual int GetMutationsPerSecond([NotNull] Hediff_MutagenicBase hediff)
        {
            return 0;
        }

        /// <summary>
        /// How many mutations to queue up for a given severity change.  Note that severity
        /// changes can be negative, and negative mutations are allowed.
        /// (negative mutations can cancel queued mutations but won't remove existing ones)
        /// 
        /// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        /// <param name="sevChange">How much severity changed by.</param>
        public virtual int GetMutationsPerSeverity([NotNull] Hediff_MutagenicBase hediff, float sevChange)
        {
            return 0;
        }

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public virtual string DebugString(Hediff_MutagenicBase hediff) => "";
    }

    /// <summary>
    /// A simple mutation rate that uses vanilla's MTB class to add roughly a given
    /// number of mutations per day.
    /// </summary>
    public class MutRate_MutationsPerDay : MutRate
    {
        /// <summary>
        /// The mean number of mutations per day. (1/the MTB of individual mutation events)
        /// </summary>
        [UsedImplicitly] public float meanMutationsPerDay;

        /// <summary>
        /// Whether or not the mutation rate is affected by mutagen sensitivity
        /// </summary>
        [UsedImplicitly] public bool affectedBySensitivity = true;

        /// <summary>
        /// How many mutations to queue up for the next second.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public override int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
        {
            float mutations = meanMutationsPerDay;
            if (affectedBySensitivity)
                mutations *= hediff.MutagenSensitivity;

            //Don't worry about division by zero, MTBEventOccurs handles positive infinity
            if (Rand.MTBEventOccurs(1f / mutations, 60000, 60))
                return 1;

            return 0;
        }

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public override string DebugString(Hediff_MutagenicBase hediff)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Mutations/Day: {meanMutationsPerDay}");
            if (affectedBySensitivity)
                builder.AppendLine($"Mutations/Day w/ Sensitivity: {meanMutationsPerDay * hediff.MutagenSensitivity}");
            return builder.ToString();
        }
    }

    /// <summary>
    /// A mutation rate that gives a normally-distributed amount of mutations based on severity changes
    /// </summary>
    public class MutRate_MutationsPerSevChange : MutRate
    {
        /// <summary>
        /// The mean number of mutations gained per point of severity.
        /// Diseases usually have 1 severity
        /// </summary>
        [UsedImplicitly] public float meanMutationsPerSeverity;

        /// <summary>
        /// The standard deviation of the mutations generated.
        /// ~68% of the time, the value will be within +/- one standard deviation of the mean
        /// ~95% of the time, the value will be within +/- two standard deviations of the mean
        /// </summary>
        [UsedImplicitly] public float standardDeviation;

        /// <summary>
        /// Whether or not the mutation rate is affected by mutagen sensitivity
        /// </summary>
        [UsedImplicitly] public bool affectedBySensitivity = true;

        /// <summary>
        /// How many mutations to queue up for a given severity change.  Note that severity
        /// changes can be negative, and negative mutations are allowed.
        /// (negative mutations can cancel queued mutations but won't remove existing ones)
        /// 
        /// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        /// <param name="sevChange">How much severity changed by.</param>
        public override int GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
        {

            float mutations = RandUtilities.generateNormalRandom(meanMutationsPerSeverity, standardDeviation);

            // Apply severity and sensitivity scaling after the random generation so
            // that deviation is scaled as well
            mutations *= sevChange;
            if (affectedBySensitivity)
                mutations *= hediff.MutagenSensitivity;

            return RandUtilities.RandRound(mutations);
        }

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public override string DebugString(Hediff_MutagenicBase hediff)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Mutations/Severity: {meanMutationsPerSeverity}");
            builder.AppendLine($"Standard Deviation: {standardDeviation}");
            if (affectedBySensitivity)
                builder.AppendLine($"Mutations/Severity w/ Sensitivity: {meanMutationsPerSeverity * hediff.MutagenSensitivity}");
            return builder.ToString();
        }
    }
}
