using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyBox;

public class AgentSystem : MonoBehaviour
{
    [SerializeField] private TerrainMesh _Terrain;

    [Space(10)]
    [SerializeField, Min(0)] private uint _ErosionAgentTotal;
    [SerializeField, Min(0)] private uint _ErosionAgentTokens;
    [Space(10)]
    [SerializeField, Min(0)] private uint _PlainAgentsTotal;
    [SerializeField, Min(0)] private uint _PlainAgentTokens;
    [Space(10)]
    [SerializeField, Min(0)] private uint _BeachAgentTotal;
    [SerializeField, Min(0)] private uint _BeachAgentTokens;
    [Space(10)]
    [SerializeField, Min(0)] private uint _RiverAgentTotal;
    [SerializeField, Min(0)] private uint _RiverAgentTokens;

    [Space(10)]
    [SerializeField] private ErosionParams _ErosionParams;
    [Space(5)]
    [SerializeField] private PlainsParams _PlainsParams;
    [Space(5)]
    [SerializeField] private BeachParams _BeachParams;
    [Space(5)]
    [SerializeField] private RiverParams _RiverParams;

    [Space(10)]
    [SerializeField]
    private AgentType[] _AgentsOrderOfExecution = new AgentType[] 
    { 
        AgentType.Erosion, AgentType.Plain, AgentType.Beach, AgentType.River 
    };

    private Graph _Graph;

    public void ResetTerrain()
    {
        _Terrain.Generate();
    }

    public async void Execute()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);

        _Graph = new Graph(_Terrain.Width, _Terrain.Height);
        _Graph.SetVertices(_Terrain.Mesh.vertices);

        for (int i = 0; i < _AgentsOrderOfExecution.Length; ++i)
        {
            switch (_AgentsOrderOfExecution[i])
            {
                case AgentType.Erosion:
                    Debug.Log("Erosion agents started");
                    await Task.Factory.StartNew(() => ExecuteAgents<ErosionAgent>());
                    Debug.Log("Erosion agents done");
                    break;
                case AgentType.Plain:
                    Debug.Log("Plain agents started");
                    await Task.Factory.StartNew(() => ExecuteAgents<PlainAgent>());
                    Debug.Log("Plain agents done");
                    break;
                case AgentType.Beach:
                    Debug.Log("Beach agents started");
                    await Task.Factory.StartNew(() => ExecuteAgents<BeachAgent>());
                    Debug.Log("Beach agents done");
                    break;
                case AgentType.River:
                    Debug.Log("River agents started");
                    await Task.Factory.StartNew(() => ExecuteAgents<RiverAgent>());
                    Debug.Log("River agents done");
                    break;
                default:
                    Debug.LogError("please assign a valid type");
                    break;
            }
        }

        ModifyTerrain();
    }

    private void ExecuteAgents<T>()
    {
        List<Agent> agents = Populate<T>();

        int completedAgents = 0;
        int totalAgents = agents.Count;

        uint totalTokens = 0;

        if (typeof(T) == typeof(ErosionAgent))
            totalTokens = _ErosionAgentTokens;
        else if (typeof(T) == typeof(PlainAgent))
            totalTokens = _PlainAgentTokens;
        else if (typeof(T) == typeof(BeachAgent))
            totalTokens = _BeachAgentTokens;
        else if (typeof(T) == typeof(RiverAgent))
            totalTokens = _RiverAgentTokens;
        else
            return;

        while (completedAgents != totalAgents)
        {
            int takenTokens = 0;

            for (int i = agents.Count - 1; i >= 0; i--)
            {
                if (takenTokens >= totalTokens)
                    break;

                bool status = agents[i].Update();

                ++takenTokens;

                if (status) // if agent is done, remove it
                {
                    ++completedAgents;
                    agents.RemoveAt(i);
                }
            }
        }
    }

    private List<Agent> Populate<T>()
    {
        List<Agent> agents = new List<Agent>();

        uint totalAgents = 0;

        if (typeof(T) == typeof(ErosionAgent))
            totalAgents = _ErosionAgentTotal;
        else if (typeof(T) == typeof(PlainAgent))
            totalAgents = _PlainAgentsTotal;
        else if (typeof(T) == typeof(BeachAgent))
            totalAgents = _BeachAgentTotal;
        else if (typeof(T) == typeof(RiverAgent))
            totalAgents = _RiverAgentTotal;
        else
            return agents;

        for (int i = 0; i < totalAgents; ++i)
        {
            if (typeof(T) == typeof(ErosionAgent))
                agents.Add(new ErosionAgent(ref _Graph, _Terrain, _ErosionParams));
            else if (typeof(T) == typeof(PlainAgent))
                agents.Add(new PlainAgent(ref _Graph, _Terrain, _PlainsParams));
            else if (typeof(T) == typeof(BeachAgent))
                agents.Add(new BeachAgent(ref _Graph, _Terrain, _BeachParams));
            else if (typeof(T) == typeof(RiverAgent))
                agents.Add(new RiverAgent(ref _Graph, _Terrain, _RiverParams));
            else
                break;

            agents[i].Initialize();
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
                vertices[x + z * _Terrain.Width].y = _Graph.AtPos(x, z).WorldPosition.y;
            }
        }
        _Terrain.Generate(vertices);
    }
}
