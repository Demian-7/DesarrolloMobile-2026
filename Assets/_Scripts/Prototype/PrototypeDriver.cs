using UnityEngine;

public class PrototypeDriver : MonoBehaviour
{
    private Board _board;
    private BoardEvaluationService _evalService;

    private void Start()
    {
        _evalService = new BoardEvaluationService();
        _board = new Board();
        
        //Create a 3x3 grid
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                _board.AddCell(x, y);
            }
        }

        RunScenario();
    }

    private void RunScenario()
    {
        Debug.Log("--- Running Phase 0 Validation Scenario ---");
        
        // Card A: Flat +10
        var generator = new CardInstance("Gen_A", new FlatGenerationBehavior(10.0));
        
        // Card B: 200% of neighbors 
        var percentage1 = new CardInstance("Perc_1", new PercentageGenerationBehavior(2.0));
        
        // Card C: 200% of neighbors
        var percentage2 = new CardInstance("Perc_2", new PercentageGenerationBehavior(2.0));
        
        // Place them in a row: Gen -> Perc1 -> Perc2
        if (!_board.TryPlaceCard(generator, new CellCoord(0, 0)))
        {
            Debug.LogError("Failed to place generator");
        }

        if (!_board.TryPlaceCard(percentage1, new CellCoord(1, 0)))
        {
            Debug.LogError("Failed to place percentage 1");
        }
        
        if (!_board.TryPlaceCard(percentage2, new CellCoord(2, 0)))
        {
            Debug.LogError("Failed to place percentage 2");
        }

        double totalRate = _evalService.EvaluateBoard(_board);

        Debug.Log($"Generator (0, 0) Effective: {generator.EffectiveValue} (Expected: 10)");
        
        // Percentage 1 sits next to the Generator
        // Reads 10 from Flat. 10 * 2.0 = 20 Effective.
        Debug.Log($"Percentage 1 (1,0) Effective: {percentage1.EffectiveValue} (Expected: 20)");
        
        // Percentage 2 sits next to Percentage 1
        // Reads 20 from Percentage 1 on the SECOND evaluation pass. 20 * 2.0 = 40 Effective.
        Debug.Log($"Percentage 2 (2,0) Effective: {percentage2.EffectiveValue} (Expected: 40)");
        
        Debug.Log($"Total Board Generation Rate: {totalRate} per second (Expected: 70)");
    }
}