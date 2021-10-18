using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyBox;

public class AgentSystem : MonoBehaviour
{
    public TerrainMesh Terrain;

    [Space(10)]
    [SerializeField, Min(0)] private uint _ErosionAgentTotal;
    [SerializeField, Min(0)] private uint _ErosionAgentTokens;
    [Space(10)]
    [SerializeField, Min(0)] private uint _PlainAgentsTotal;
    [SerializeField, Min(0)] private uint _PlainAgentTokens;
    [Space(10)]
    [SerializeField, Min(0)] private uint _RiverAgentTotal;
    [SerializeField, Min(0)] private uint _RiverAgentTokens;

    [Space(10)]
    [SerializeField] private ErosionParams _ErosionParams;
    [Space(5)]
    [SerializeField] private PlainsParams _PlainsParams;
    [Space(5)]
    [SerializeField] private RiverParams _RiverParams;

    [Space(10)]
    [SerializeField]
    private AgentType[] _AgentsOrderOfExecution = new AgentType[] 
    { 
        AgentType.Erosion, AgentType.Plain, AgentType.River 
    };

    private Graph _Graph;

    public void ResetTerrain()
    {
        Terrain.Generate();
    }

    public async void Execute()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);

        _Graph = new Graph(Terrain.Width, Terrain.Height);
        _Graph.SetVertices(Terrain.Mesh.vertices);

        for (int i = 0; i < _AgentsOrderOfExecution.Length; ++i)
        {
            AgentType agentType = _AgentsOrderOfExecution[i];

            Debug.Log(System.Enum.GetName(typeof(AgentType), agentType) + " started");
            await Task.Factory.StartNew(() => ExecuteAgents(agentType));
            Debug.Log(System.Enum.GetName(typeof(AgentType), agentType) + " stopped");
        }

        ModifyTerrain();
    }

    private void ExecuteAgents(AgentType agentType)
    {
        List<Agent> agents = Populate(agentType);

        int completedAgents = 0;
        int totalAgents = agents.Count;

        uint totalTokens = 0;

        switch (agentType)
        {
            case AgentType.Erosion:
                totalTokens = _ErosionAgentTokens;
                break;
            case AgentType.Plain:
                totalTokens = _PlainAgentTokens;
                break;
            case AgentType.River:
                totalTokens = _RiverAgentTokens;
                break;
        }

        System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
        double secondFrame = stopWatch.Elapsed.TotalMilliseconds;

        while (completedAgents != totalAgents)
        {
            int takenTokens = 0;

            System.TimeSpan elapsedTime = stopWatch.Elapsed;
            double firstFrame = elapsedTime.TotalMilliseconds;

            double deltaTime = (firstFrame - secondFrame) / 1000.0;

            for (int i = agents.Count - 1; i >= 0; i--)
            {
                if (takenTokens >= totalTokens)
                    break;

                ++takenTokens;

                bool status = agents[i].Update((float)deltaTime);

                if (status) // if agent is done, remove it
                {
                    ++completedAgents;
                    agents.RemoveAt(i);
                }
            }

            secondFrame = elapsedTime.TotalMilliseconds;
        }
    }

    private List<Agent> Populate(AgentType agentType)
    {
        List<Agent> agents = new List<Agent>();

        uint totalAgents = 0;
        
        switch (agentType)
        {
            case AgentType.Erosion:
                totalAgents = _ErosionAgentTotal;
                break;
            case AgentType.Plain:
                totalAgents = _PlainAgentsTotal;
                break;
            case AgentType.River:
                totalAgents = _RiverAgentTotal;
                break;
        }

        for (int i = 0; i < totalAgents; ++i)
        {
            switch (agentType)
            {
                case AgentType.Erosion:
                    agents.Add(new ErosionAgent(ref _Graph, Terrain, _ErosionParams));
                    break;
                case AgentType.Plain:
                    agents.Add(new PlainAgent(ref _Graph, Terrain, _PlainsParams));
                    break;
                case AgentType.River:
                    agents.Add(new RiverAgent(ref _Graph, Terrain, _RiverParams));
                    break;
            }

            agents[i].Initialize();
        }

        return agents;
    }

    private void ModifyTerrain()
    {
        Vector3[] vertices = Terrain.Mesh.vertices;
        for (int x = 0; x < Terrain.Width; ++x)
        {
            for (int z = 0; z < Terrain.Height; ++z)
            {
                vertices[x + z * Terrain.Width].y = _Graph.AtPos(x, z).WorldPosition.y;
            }
        }
        Terrain.Generate(vertices);
    }

    public float[] RandomExecution(float[] heightmap, int width, int height)
    {
        _ErosionAgentTotal = (uint)StaticRandom.Range(500, 2250);
        _ErosionAgentTokens = (uint)StaticRandom.Range(1, _ErosionAgentTotal);

        _PlainAgentsTotal = (uint)StaticRandom.Range(2, 5);
        _PlainAgentTokens = (uint)StaticRandom.Range(1, _PlainAgentsTotal);

        _RiverAgentTotal = (uint)StaticRandom.Range(2, 5);
        _RiverAgentTokens = (uint)StaticRandom.Range(1, _RiverAgentTotal);

        _ErosionParams = new ErosionParams();
        _PlainsParams = new PlainsParams();
        _RiverParams = new RiverParams();

        _ErosionParams.Randomize();
        _PlainsParams.Randomize();
        _RiverParams.Randomize();

        _Graph = new Graph(width, height);
        _Graph.SetVertices(heightmap);

        List<AgentType> orderOfExecution = new List<AgentType>();

        for (int i = 0; i < StaticRandom.Range(1, 5); ++i)
        {
            System.Array values = System.Enum.GetValues(typeof(AgentType));
            orderOfExecution.Add((AgentType)values.GetValue(StaticRandom.Range(0, values.Length)));

            ExecuteAgents(orderOfExecution[i]);
        }

        return System.Array.ConvertAll(_Graph.Vertices, v => v.WorldPosition.y);
    }
}
