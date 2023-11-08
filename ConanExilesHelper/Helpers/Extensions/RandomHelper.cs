using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ConanExilesHelper.Helpers.Extensions;

public static class RandomHelper
{
    // Methods from
    // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rngcryptoserviceprovider?redirectedfrom=MSDN&view=netcore-3.1#Anchor_3

    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public static T PickSecureRandom<T>(IList<T> list)
    {
        return PickSecureRandom(list, out var _);
    }

    public static T PickSecureRandom<T>(IList<T> list, out byte index)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));
        if (list.Count >= byte.MaxValue)
        {
            throw new ArgumentException($"The list can't have more than {byte.MaxValue} sides.", nameof(list));
        }

        if (list.Count == 1)
        {
            index = 0;
            return list[0];
        }

        index = SecureRollDice((byte) list.Count);

        index--;

        var item = list[index];
        return item;
    }

    /// <summary>
    /// Gets a random number between 1 and <paramref name="numberSides" /> INCLUSIVE! You should subtract one if you want
    /// to pick from an array.
    /// </summary>
    /// <param name="rngCsp">The RNG provider to use.</param>
    /// <param name="numberSides">The number of sides on the die to roll.</param>
    /// <returns>The number from a random dice roll, between 1 and <paramref name="numberSides" /> inclusive.</returns>
    public static byte SecureRollDice(byte numberSides)
    {
        if (numberSides <= 1) throw new ArgumentOutOfRangeException(nameof(numberSides), "Value must be >= 2.");

        // Create a byte array to hold the random value.
        byte[] randomNumber = new byte[1];
        do
        {
            // Fill the array with a random value.
            Rng.GetBytes(randomNumber);
        }
        while (!IsFairRoll(randomNumber[0], numberSides));
        // Return the random number mod the number
        // of sides. The possible values are zero-
        // based, so we add one.
        return (byte)((randomNumber[0] % numberSides) + 1);
    }

    private static bool IsFairRoll(byte roll, byte numSides)
    {
        // There are MaxValue / numSides full sets of numbers that can come up
        // in a single byte.  For instance, if we have a 6 sided die, there are
        // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
        int fullSetsOfValues = byte.MaxValue / numSides;

        // If the roll is within this range of fair values, then we let it continue.
        // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
        // < rather than <= since the = portion allows through an extra 0 value).
        // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
        // to use.
        return roll < numSides * fullSetsOfValues;
    }
}
