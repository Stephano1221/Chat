using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public static class VersionNumber
    {
        public static int[] SplitVersionNumberPrefix(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return null;
            }
            string versionNumberToSplitPrefix = versionNumberToSplit.Split('-', 2)[0];
            string[] versionNumbersSplitAsString = versionNumberToSplitPrefix.Split('.');
            int[] VersionNumbersSplitAsInt = new int[versionNumbersSplitAsString.Count()];
            for (int i = 0; i < versionNumbersSplitAsString.Count(); i++)
            {
                int.TryParse(versionNumbersSplitAsString[i], out VersionNumbersSplitAsInt[i]);
            }
            return VersionNumbersSplitAsInt;
        }

        public static string GetPreReleaseNumberFromVersionNumber(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return versionNumberToSplit;
            }
            string preReleaseNumber = null;
            char seperator = '-';
            string[] preReleaseNumberParts = versionNumberToSplit.Split(seperator, 2);
            if (preReleaseNumberParts.Count() > 1)
            {
                preReleaseNumber = preReleaseNumberParts[1];
            }
            return preReleaseNumber;
        }


        public static string removeBuildInfoFromVersionNumber(string versionNumberToSplit)
        {
            if (string.IsNullOrWhiteSpace(versionNumberToSplit))
            {
                return versionNumberToSplit;
            }
            string versionNumberWithoutBuildInfo = versionNumberToSplit;
            char seperator = '+';
            string[] versionNumberParts = versionNumberToSplit.Split(seperator, 2);
            if (versionNumberWithoutBuildInfo.Count() > 1)
            {
                versionNumberWithoutBuildInfo = versionNumberParts[0];
            }
            return versionNumberWithoutBuildInfo;
        }

        public static char DeterminePreReleaseVersionNumberPrecedence(string basePreReleaseVersionNumber, string challengePreReleaseVersionNumber, bool allowPreRelease)
        {
            if (basePreReleaseVersionNumber == null && challengePreReleaseVersionNumber == null)
            {
                return '=';
            }
            else if ((basePreReleaseVersionNumber == null || allowPreRelease == false) && challengePreReleaseVersionNumber != null)
            {
                return '<';
            }
            else if (basePreReleaseVersionNumber != null && challengePreReleaseVersionNumber == null)
            {
                return '>';
            }
            char identifierSeperator = '.';
            string[] basePreReleaseVersionNumberIdentifiers = basePreReleaseVersionNumber.Split(identifierSeperator);
            string[] challengePreReleaseVersionNumberIdentifiers = challengePreReleaseVersionNumber.Split(identifierSeperator);
            int[] preReleaseIdentifierCount = { basePreReleaseVersionNumberIdentifiers.Count(), challengePreReleaseVersionNumberIdentifiers.Count() };
            int smallestPreReleaseIdentifierCount = preReleaseIdentifierCount.Min();
            for (int i = 0; i < smallestPreReleaseIdentifierCount; i++)
            {
                int basePreReleaseVersionNumberIdentifier;
                int challengePreReleaseVersionNumberIdentifier;
                if (Int32.TryParse(basePreReleaseVersionNumberIdentifiers[i], out basePreReleaseVersionNumberIdentifier) && Int32.TryParse(challengePreReleaseVersionNumberIdentifiers[i], out challengePreReleaseVersionNumberIdentifier))
                {
                    if (basePreReleaseVersionNumberIdentifier > challengePreReleaseVersionNumberIdentifier)
                    {
                        return '<';
                    }
                    else if (basePreReleaseVersionNumberIdentifier < challengePreReleaseVersionNumberIdentifier)
                    {
                        return '>';
                    }
                }
                else
                {
                    int[] preReleaseIdentifierCharacterCount = { basePreReleaseVersionNumberIdentifiers[i].Count(), challengePreReleaseVersionNumberIdentifiers[i].Count() };
                    int smallestPreReleaseIdentifierCharacterCount = preReleaseIdentifierCharacterCount.Min();
                    for (int j = 0; j < smallestPreReleaseIdentifierCharacterCount; j++)
                    {
                        int basePreReleaseCharacterCode = basePreReleaseVersionNumberIdentifiers[i][j];
                        int challengePreReleaseCharacterCode = challengePreReleaseVersionNumberIdentifiers[i][j];
                        if (basePreReleaseCharacterCode > challengePreReleaseCharacterCode)
                        {
                            return '<';
                        }
                        else if (basePreReleaseCharacterCode < challengePreReleaseCharacterCode)
                        {
                            return '>';
                        }
                    }
                    if (preReleaseIdentifierCharacterCount[0] > preReleaseIdentifierCharacterCount[1])
                    {
                        return '<';
                    }
                    else if (preReleaseIdentifierCharacterCount[0] < preReleaseIdentifierCharacterCount[1])
                    {
                        return '>';
                    }
                }
            }
            if (preReleaseIdentifierCount[0] > preReleaseIdentifierCount[1])
            {
                return '<';
            }
            else if (preReleaseIdentifierCount[0] < preReleaseIdentifierCount[1])
            {
                return '>';
            }
            return '=';
        }

        public static char CheckVersionCompatibility(string minimumVersionNumber, string maximumVersionNumber, string challengeVersionNumber, bool allowPreRelease)
        {
            char minimumVersionNumberDifference = CompareVersionNumber(minimumVersionNumber, challengeVersionNumber, allowPreRelease);
            if (minimumVersionNumberDifference == '<')
            {
                return minimumVersionNumberDifference;
            }
            char maximumVersionNumberDifference = CompareVersionNumber(maximumVersionNumber, challengeVersionNumber, allowPreRelease);
            if (maximumVersionNumberDifference == '>')
            {
                return maximumVersionNumberDifference;
            }
            return '=';
        }

        public static char CompareVersionNumber(string baseVersionNumber, string challengeVersionNumber, bool allowPreRelease)
        {
            if (string.IsNullOrWhiteSpace(baseVersionNumber) || string.IsNullOrWhiteSpace(challengeVersionNumber))
            {
                return '=';
            }
            string baseVersionNumberWithoutBuildInfo = removeBuildInfoFromVersionNumber(baseVersionNumber);
            string challengeVersionNumberWithoutBuildInfo = removeBuildInfoFromVersionNumber(challengeVersionNumber);
            int[] baseVersionNumberSplit = SplitVersionNumberPrefix(baseVersionNumberWithoutBuildInfo);
            int[] challengeVersionNumberSplit = SplitVersionNumberPrefix(challengeVersionNumberWithoutBuildInfo);
            if (baseVersionNumberSplit[0] != challengeVersionNumberSplit[0]) // Incompatible
            {
                char versionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[0], challengeVersionNumberSplit[0]);
                return versionDifference;
            }
            if (baseVersionNumberSplit[0] == 0 || challengeVersionNumberSplit[0] == 0)
            {
                if (baseVersionNumberSplit[1] != challengeVersionNumberSplit[1]) // Incompatible
                {
                    char versionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[1], challengeVersionNumberSplit[1]);
                    return versionDifference;
                }
            }
            int[] versionNumberPrefixIdentifierCount = { baseVersionNumberSplit.Count(), challengeVersionNumberSplit.Count() };
            int smallestVersionNumberPrefixIdentifierCount = versionNumberPrefixIdentifierCount.Min();
            for (int i = 1; i < smallestVersionNumberPrefixIdentifierCount; i++)
            {
                char VersionDifference = CompareIndividualVersionNumber(baseVersionNumberSplit[i], challengeVersionNumberSplit[i]);
                if (VersionDifference != '=') // Incompatible
                {
                    return VersionDifference;
                }
            }
            string basePreReleaseVersionNumber = GetPreReleaseNumberFromVersionNumber(baseVersionNumberWithoutBuildInfo);
            if (basePreReleaseVersionNumber != null)
            {
                allowPreRelease = true;
            }
            string challengePreReleaseVersionNumber = GetPreReleaseNumberFromVersionNumber(challengeVersionNumberWithoutBuildInfo);
            char preReleaseVersionDifference = DeterminePreReleaseVersionNumberPrecedence(basePreReleaseVersionNumber, challengePreReleaseVersionNumber, allowPreRelease);
            return preReleaseVersionDifference;
        }

        public static char CompareIndividualVersionNumber(int minimumVersionNumber, int challengeVersionNumber)
        {
            if (minimumVersionNumber < challengeVersionNumber)
            {
                return '>';
            }
            else if (minimumVersionNumber > challengeVersionNumber)
            {
                return '<';
            }
            return '=';
        }
    }
}
