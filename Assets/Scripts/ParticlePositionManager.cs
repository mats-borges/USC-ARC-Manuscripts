using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Obi;
using UnityEditor;
using UnityEngine;

public class ParticlePositionManager : MonoBehaviour
{
    [SerializeField] private ObiActor actor;
    [SerializeField] private string saveFileName = "ParticlePositions";
    private ObiSolver _obiSolver;
    private int particleCount;
    [SerializeField] private string startupFileName = "RightSideResting";
    [SerializeField] private bool loadFileOnStartup = false;

    private void Awake()
    {
        _obiSolver = FindObjectOfType<ObiSolver>();
    }

    private void Start()
    {
        if (!loadFileOnStartup) return;

        LoadParticles(startupFileName);
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveParticles();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadParticles(saveFileName);
        }
        #endif
    }
    
    TextAsset ConvertStringToTextAsset(string text) {
#if UNITY_EDITOR
        File.WriteAllText(Application.dataPath +  "/Resources/" + saveFileName + ".txt", text);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        var textAsset = Resources.Load(saveFileName) as TextAsset;
        return textAsset;
#else
    return new TextAsset();
#endif
    }

    public void SaveParticles()
    {
        if (_obiSolver == null) return;
        if (actor == null) return;

        particleCount = actor.particleCount;
        var obiTransform = _obiSolver.transform;
        
        var sb = new StringBuilder();
        for (var i = 0; i < particleCount; ++i)
        {
            // get particle position (world-space)
            var particlePos = (actor.GetParticlePosition(i));
            
            // convert particle position from world space to space of the obi solver
            particlePos = obiTransform.InverseTransformPoint(particlePos);

            sb.AppendFormat("{0}", particlePos.ToString());
            sb.AppendLine();
        }
        
        ConvertStringToTextAsset(sb.ToString());
    }

    public void LoadParticles(string fileName)
    {
        StartCoroutine(LoadParticlesRoutine(fileName));
    }
    private IEnumerator LoadParticlesRoutine(string fileName)
    {
        if (actor == null) yield return null;

        if (Resources.Load(fileName) is TextAsset { } file)
        {
            var fs = file.text;
            var fileLines = Regex.Split ( fs, "\n|\r|\r\n" );
            particleCount = actor.particleCount;
            var particleIdx = 0;
 
            var pattern = @"\(([+-]?[0-9]*[.]?[0-9]+), ([+-]?[0-9]*[.]?[0-9]+), ([+-]?[0-9]*[.]?[0-9]+)\)";
            foreach (var line in fileLines)
            {
                foreach (Match match in Regex.Matches(line, pattern))
                {
                    var groups = match.Groups;
                    var xPos = float.Parse(groups[1].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    var yPos = float.Parse(groups[2].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    var zPos = float.Parse(groups[3].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    var newParticlePos = new Vector3(xPos, yPos, zPos);
                
                    actor.TeleportParticle(particleIdx++, newParticlePos);
                }
            }
        }

        yield return null;
    }
}
