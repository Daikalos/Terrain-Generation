using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyBox;

public class AgentSystem : MonoBehaviour
{
    [SerializeField]
    private TerrainMesh _Terrain;

    [Space(10)]
    [SerializeField, Min(0)]
    private uint _FlattenAgentTokens;
    [SerializeField, Min(0)]
    private uint _BeachAgentTokens;
    [SerializeField, Min(0)]
    private uint _ErosionAgentTokens;
    [SerializeField, Min(0)]
    private uint _RiverAgentTokens;

    [Space(10)]
    [SerializeField]
    private PlainAgent _PlainsParams;
    [SerializeField]
    private BeachAgent _BeachParams;
    [SerializeField]
    private ErosionAgent _ErosionParams;
    [SerializeField]
    private RiverAgent _RiverParams;

    private Graph _Graph;

    public async void Execute()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);

        _Graph = new Graph(_Terrain.Width, _Terrain.Height);
        _Graph.SetVertices(_Terrain.Mesh.vertices);

        Debug.Log("Plain agents started");
        await Task.Factory.StartNew(() => ExecuteAgents<PlainAgent>());
        Debug.Log("Plain agents done");

        Debug.Log("Beach agents started");
        await Task.Factory.StartNew(() => ExecuteAgents<BeachAgent>());
        Debug.Log("Beach agents done");

        Debug.Log("Erosion agents started");
        await Task.Factory.StartNew(() => ExecuteAgents<ErosionAgent>());
        Debug.Log("Erosion agents done");

        Debug.Log("River agents started");
        await Task.Factory.StartNew(() => ExecuteAgents<RiverAgent>());
        Debug.Log("River agents done");

        ModifyTerrain();
    }

    private void ExecuteAgents<T>()
    {
        List<Agent> agents = Populate<T>();
        int complete = 0;

        int numAgents = agents.Count;
        while (complete != numAgents)
        {
            for (int i = agents.Count - 1; i >= 0; i--)
            {
                bool status = agents[i].Update();

                if (status) // if agent is done, remove it
                {
                    ++complete;
                    agents.RemoveAt(i);
                }
            }
        }
    }

    private List<Agent> Populate<T>()
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < StaticRandom.Range(1, 1000); ++i)
        {
            if (typeof(T) == typeof(PlainAgent))
                agents.Add(_PlainsParams.Clone() as PlainAgent);
            else if (typeof(T) == typeof(BeachAgent))
                agents.Add(_BeachParams.Clone() as BeachAgent);
            else if (typeof(T) == typeof(ErosionAgent))
                agents.Add(_ErosionParams.Clone() as ErosionAgent);
            else if (typeof(T) == typeof(RiverAgent))
                agents.Add(_RiverParams.Clone() as RiverAgent);
            else
                break;

            agents[i].Initialize(ref _Graph);
        }

        return agents;
    }

    private void ModifyTerrain()
    {
        Vector3[] vertices = _Terrain.Mesh.vertices;
        for (int x = 0; x < _Terrain.Width; ++x)
        {
            for (int z = 0; z < _Terrain.Height; ++z)
            {
                vertices[x + z * _Terrain.Width].y = _Graph.AtPos(x, z).Position.y;
            }
        }
        _Terrain.Mesh.vertices = vertices;
    }
}
