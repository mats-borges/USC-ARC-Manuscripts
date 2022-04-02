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
    [SerializeField] public TextAsset ParticlePositionFile;
    [SerializeField] private ObiActor actor;
    private ObiSolver _obiSolver;
    private int particleCount;

    private void Awake()
    {
        _obiSolver = FindObjectOfType<ObiSolver>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveParticles();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadParticles();
        }
    }
    
    TextAsset ConvertStringToTextAsset(string text) {
        string temporaryTextFileName = "ParticlePositions";
        File.WriteAllText(Application.dataPath +  "/Resources/" + temporaryTextFileName + ".txt", text);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        TextAsset textAsset = Resources.Load(temporaryTextFileName) as TextAsset;
        return textAsset;
    }

    public void SaveParticles()
    {
        if (_obiSolver == null) return;
        if (actor == null) return;

        particleCount = actor.particleCount;
        
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < particleCount; ++i)
        {
            var particlePos = (actor.GetParticlePosition(i) - actor.transform.position) / _obiSolver.transform.localScale.x;
            sb.AppendFormat("{0}", particlePos.ToString());
            sb.AppendLine();
        }

        ParticlePositionFile = ConvertStringToTextAsset(sb.ToString());
    }

    public void LoadParticles()
    {
        StartCoroutine(LoadParticlesRoutine());
    }
    private IEnumerator LoadParticlesRoutine()
    {
        if (actor == null) yield return null;
        if (ParticlePositionFile == null) yield return null;

        var fs = ParticlePositionFile.text;
        var fileLines = Regex.Split ( fs, "\n|\r|\r\n" );
        particleCount = actor.particleCount;
        var particleIdx = 0;
 
        for (var i = 0; i < fileLines.Length; ++i ) {
 
            var valueLine = fileLines[i];
            var pattern = @"\(([+-]?[0-9]*[.]?[0-9]+), ([+-]?[0-9]*[.]?[0-9]+), ([+-]?[0-9]*[.]?[0-9]+)\)";

            foreach (Match match in Regex.Matches(valueLine, pattern))
            {
                GroupCollection groups = match.Groups;
                var xPos = float.Parse(groups[1].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                var yPos = float.Parse(groups[2].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                var zPos = float.Parse(groups[3].ToString(), CultureInfo.InvariantCulture.NumberFormat);
                var newParticlePos = new Vector3(xPos, yPos, zPos);
                
                actor.TeleportParticle(particleIdx++, newParticlePos);
            }
        }
        yield return null;
    }
}
