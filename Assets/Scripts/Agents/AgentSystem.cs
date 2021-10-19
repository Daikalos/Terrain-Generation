using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MyBox;

public class AgentSystem : MonoBehaviour
{
    public TerrainMesh Terrain;

    [SerializeField]
    private AgentOrderOfExecution[] _AgentsOrderOfExecution = new AgentOrderOfExecution[]
    {
        new AgentOrderOfExecution{ AgentType = AgentType.Erosion },
        new AgentOrderOfExecution{ AgentType = AgentType.Plain }
    };

    [Space(10)]
    [SerializeField] private ErosionParams _ErosionParams;
    [Space(5)]
    [SerializeField] private PlainParams _PlainParams;

    private Graph _Graph;

    public void RestoreTerrain()
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

        ModifyTerrain();

        for (int i = 0; i < _AgentsOrderOfExecution.Length; ++i)
        {
            AgentOrderOfExecution agentData = _AgentsOrderOfExecution[i];

            Debug.Log(System.Enum.GetName(typeof(AgentType), agentData.AgentType) + " started");
            await Task.Factory.StartNew(() => ExecuteAgents(agentData.AgentType, agentData.TotalAgents, agentData.TotalConcurrent));
            Debug.Log(System.Enum.GetName(typeof(AgentType), agentData.AgentType) + " stopped");

            ModifyTerrain();
        }
    }

    private void ExecuteAgents(AgentType agentType, uint totalAgents, uint totalConcurrent)
    {
        if (totalConcurrent <= 0 || totalAgents <= 0)
            return;

        List<Agent> agents = Populate(agentType, totalAgents);
        int completedAgents = 0;

        System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
        double secondFrame = stopWatch.Elapsed.TotalMilliseconds;

        while (completedAgents != totalAgents)
        {
            int activeAgents = 0;

            System.TimeSpan elapsedTime = stopWatch.Elapsed;
            double firstFrame = elapsedTime.TotalMilliseconds;

            double deltaTime = (firstFrame - secondFrame) / 1000.0;

            for (int i = agents.Count - 1; i >= 0; i--)
            {
                if (activeAgents >= totalConcurrent)
                    break;

                ++activeAgents;

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

    private List<Agent> Populate(AgentType agentType, uint totalAgents)
    {
        List<Agent> agents = new List<Agent>();

        for (int i = 0; i < totalAgents; ++i)
        {
            switch (agentType)
            {
                case AgentType.Erosion:
                    agents.Add(new ErosionAgent(ref _Graph, Terrain, _ErosionParams));
                    break;
                case AgentType.Plain:
                    agents.Add(new PlainAgent(ref _Graph, Terrain, _PlainParams));
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
        uint erosionAgentTotal = (uint)StaticRandom.Range(500, 1500);
        uint erosionAgentConcurrent = (uint)StaticRandom.Range(1, erosionAgentTotal);

        uint plainAgentTotal = (uint)StaticRandom.Range(1, 4);
        uint plainAgentConcurrent = (uint)StaticRandom.Range(1, plainAgentTotal);

        ErosionParams erosionParams = _ErosionParams;
        PlainParams plainParams = _PlainParams;

        _ErosionParams.Randomize();
        _PlainParams.Randomize();

        _Graph = new Graph(width, height);
        _Graph.SetVertices(heightmap);

        for (int i = 0; i < StaticRandom.Range(1, 4); ++i)
        {
            System.Array values = System.Enum.GetValues(typeof(AgentType));
            AgentType randomType = (AgentType)values.GetValue(StaticRandom.Range(0, values.Length));

            uint totalAgents = 0;
            uint totalConcurrent = 0;

            switch (randomType)
            {
                case AgentType.Erosion:
                    totalAgents = erosionAgentTotal;
                    totalConcurrent = erosionAgentConcurrent;
                    break;
                case AgentType.Plain:
                    totalAgents = plainAgentTotal;
                    totalConcurrent = plainAgentConcurrent;
                    break;
            }

            ExecuteAgents(randomType, totalAgents, totalConcurrent);
        }

        _ErosionParams = erosionParams;
        _PlainParams = plainParams;

        return System.Array.ConvertAll(_Graph.Vertices, v => v.WorldPosition.y);
    }

    [System.Serializable]
    private class AgentOrderOfExecution
    {
        public AgentType AgentType;
        [Min(0)] public uint TotalAgents;
        [Min(0)] public uint TotalConcurrent; // total tokens that can be used by agents
    }
}
