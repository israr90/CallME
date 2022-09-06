using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMe.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class ProfileRepository : Repository<Profile>, IProfileRepository
    {
        ApplicationDbContext _context;
        internal ProfileRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Profile> Filter(string keyword)
        {
            int areaId = 0;
            if (int.TryParse(keyword, out areaId))
            {
                var area = FindById(areaId); ;
                if (area != null)
                {
                    var singleItemList = new List<Profile> { area };
                    return singleItemList;
                }
            }

            return Set.Where(c => c.Name.Contains(keyword) || c.ProfileType.Name.Contains(keyword));
        }

        public int GenerateId()
        {
            return (Set.Select(x => (int?)x.ProfileId).Max() ?? 0) + 1;
        }

        public Profile GetByShortName(string prefix)
        {
            return Set.Where(m => m.ShortName == prefix)?.FirstOrDefault();
        }

        public IEnumerable<Profile> GetByProfileTypeId(int profileTypeId)
        {
            return Set.Where(m => m.ProfileTypeId == profileTypeId)?.ToList();
        }
        public async Task<BoolResultVM> GetPrivatePolicy()
        {
            try
            {
                var result = Set.FirstOrDefault(x => x.ShortName == "PRVPOLCY")?.Description;
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200", Data = result });
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new BoolResultVM { StatusCode = "409", IsSuccessed = false });
            }
        }
        public async Task<BoolResultVM> TermsAndConditions()
        {
            try
            {
                var result = Set.FirstOrDefault(x => x.ShortName == "TRMCONDS")?.Description;
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200", Data = result });
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new BoolResultVM { StatusCode = "409", IsSuccessed = false });
            }
        }
        //public async Task<List<FAQSVM>> FAQS()
        //{
        //    try
        //    {
        //        var resultList = new List<FAQSVM>();
        //        var faqsLst = Set.Where(x => x.ShortName == "FAQS")?.Select(x => x.Description)?.ToList();
        //        foreach (var item in faqsLst)
        //        {
        //            var objFAQ = new FAQSVM();
        //            objFAQ.Question = GetSubString(item, "<QUS>", "</QUS>");
        //            objFAQ.Answer = GetSubString(item, "<ANS>", "</ANS>");
        //            resultList.Add(objFAQ);
        //        }
        //        return await Task.FromResult(resultList);
        //    }
        //    catch (Exception ex)
        //    {
        //        return await Task.FromResult(new List<FAQSVM>());
        //    }
        //}
        public string GetSubString(string message, string first, string last)
        {
            try
            {
                string resString = "";
                int Start, End;
                if (message.Contains(first) && message.Contains(last))
                {
                    Start = message.IndexOf(first, 0) + first.Length;
                    End = message.IndexOf(last, Start);
                    resString = message.Substring(Start, End - Start);
                }
                return resString;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
