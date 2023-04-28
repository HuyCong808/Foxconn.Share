using System.Collections.Generic;

namespace Foxconn.TestUI.Config
{
    public class Board
    {
        public string Name { get; set; }
        public List<FOV> FOVs { get; set; }

        public Board()
        {
            Name = "";
            FOVs = new List<FOV>();
        }

        public void AddFOV()
        {
            int id = FOVs.Count;
            FOV item = new FOV()
            {
                ID = id,
                Name = $"FOV_{id}"
            };
            FOVs.Add(item);
        }

        public void AddSMD(int fovId)
        {
            var currentFOV = FOVs.Find(x => x.ID == fovId);
            if (currentFOV != null)
            {
                var newId = currentFOV.SMDs.Count;
                var smd = new SMD()
                {
                    Id = newId,
                    Name = $"SMD_{newId}"
                };
                currentFOV.SMDs.Add(smd);
            }
        }

        public void SortFOV()
        {
            //int i=0;
            //foreach(var fov in FOVs)
            //{
            //    fov.Id = i;
            //    fov.Name = $"FOV_{i}";
            //    ++i;
            //}

            for (int j = 0; j < FOVs.Count; j++)
            {
                FOVs[j].ID = j;
                FOVs[j].Name = $"FOV_{j}";
            }
        }

        public void SortSMD(int fovid)
        {
            var cunrentFOV = FOVs.Find(x => x.ID == fovid);
            int i = 0;
            if (cunrentFOV != null)
            {
                foreach (var smd in FOVs[fovid].SMDs)
                {
                    smd.Id = i;
                    smd.Name = $"SMD_{i}";
                    ++i;
                }
            }
        }
    }
}
