using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace foneMe.SL.Utilities
{
    public static class HelperFunctions
    {
        public static double RoundOff(this double num)
        {
            double ans = 0;
            //if (Math.Round(num % 10.0) > 5) ans = (Math.Round(num / 10.0)) * 10;
            //else ans = Math.Round(num);
            ans = Math.Round(num, 0);
            return ans;
        }

        public static double RoundOff(this decimal? num)
        {
            double ans = 0;
            double val = Convert.ToDouble(num);
            //if (Math.Round(val % 10.0) > 5) ans = (Math.Round(val / 10.0)) * 10;
            //else ans = Math.Round(val);
            ans = Math.Round(val, 0);
            return ans;
        }

        public static string FormatPhoneNumber(string phoneNum, string phoneFormat)
        {
            string subString = "";
            if (phoneFormat == "")
            {
                // If phone format is empty, code will use default format (###) ###-####
                phoneFormat = "#########";
            }

            // First, remove everything except of numbers
            Regex regexObj = new Regex(@"[^\d]");
            phoneNum = regexObj.Replace(phoneNum, "");

            if (phoneNum.Length > 0)
            {
                phoneNum = Convert.ToInt64(phoneNum).ToString(phoneFormat);
            }

            // Second, format numbers to phone string
            if (phoneNum.Length > 10)
            {
                var toDelete = phoneNum.Length - 10;
                subString = phoneNum.Substring(toDelete);
            }
            else
            {
                subString = phoneNum;
            }

            return subString;
        }



        /// <summary>
        /// Calculate Age From Given Date to Today
        /// </summary>
        /// <param name="dateOfBirth">Start from calculate age</param>
        /// <returns></returns>
        public static string Calculate_Age(DateTime? dateOfBirth)
        {
            try
            {
                if (dateOfBirth == null)
                {
                    return "";
                }
                DateTime now = DateTime.Today;

                var days = now.Day - dateOfBirth.Value.Day;
                if (days < 0)
                {
                    var newNow = now.AddMonths(-1);
                    days += (int)(now - newNow).TotalDays;
                    now = newNow;
                }
                var months = now.Month - dateOfBirth.Value.Month;
                if (months < 0)
                {
                    months += 12;
                    now = now.AddYears(-1);
                }
                var years = now.Year - dateOfBirth.Value.Year;
                if (years == 0)
                {
                    if (months == 0)
                        return days.ToString() + " D";
                    else
                        return months.ToString() + " M";
                }
                return years.ToString() + " Y";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string GenerateUniqueId()
        {
            try
            {
                string id = DateTime.Now.ToString("yyMMddHHmmssff");
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
