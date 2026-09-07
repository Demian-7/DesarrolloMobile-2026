// using System.Collections.Generic;
// using System.Linq;
//
// public class BoardEvaluationService
// {
//     public double EvaluateBoard(Board board)
//     {
//         var allCards = board.Cells.Values
//             .Where(c => c.OccupiedCard != null)
//             .Select(c => c.OccupiedCard)
//             .ToList();
//         
//         //Reset State
//         foreach (var card in allCards)
//         {
//             card.BaseValue = 0;
//             card.EffectiveValue = 0;
//             card.IsResolved = false;
//         }
//
//         var resolvedCards = new HashSet<CardInstance>();
//         
//         //Flat Phase
//         foreach (var card in allCards)
//         {
//             var flat = card.GetBehavior<FlatGenerationBehavior>();
//             if (flat != null)
//             {
//                 card.BaseValue = flat.BaseAmount;
//                 card.EffectiveValue = card.BaseValue * CalculateModifierMultiplier(board, card.CurrentCell);
//                 card.IsResolved = true;
//                 resolvedCards.Add(card);
//             }
//         }
//         
//         //Percentage Phase (Iterative Wavefront)
//         bool changed = true;
//         while (changed)
//         {
//             changed = false;
//             var snapshot = new HashSet<CardInstance>(resolvedCards);
//
//             foreach (var card in allCards.Where(c => !snapshot.Contains(c)))
//             {
//                  var percent = card.GetBehavior<PercentageGenerationBehavior>();
//                  if (percent != null)
//                  {
//                      var neighbors = board.GetNeighbors(card.CurrentCell.Coordinate);
//                      var resolvedNeighbors = neighbors
//                          .Where(n => n.OccupiedCard != null && snapshot.Contains(n.OccupiedCard))
//                          .ToList();
//
//                      if (resolvedNeighbors.Count > 0)
//                      {
//                          double neighborSum = resolvedNeighbors.Sum(n => n.OccupiedCard.EffectiveValue);
//                          card.BaseValue = neighborSum * percent.Percentage;
//                          card.EffectiveValue = card.BaseValue * CalculateModifierMultiplier(board, card.CurrentCell);
//
//                          card.IsResolved = true;
//                          resolvedCards.Add(card);
//                          changed = true;
//                      }
//                  }
//             }
//         }
//         
//         //Anything still unresolved (cycles with no Flat source) stays at 0.
//         return allCards.Sum(c => c.EffectiveValue);
//     }
//
//     private double CalculateModifierMultiplier(Board board, Cell cell)
//     {
//         double totalMultiplier = 1.0;
//         var neighbors = board.GetNeighbors(cell.Coordinate);
//
//         foreach (var neighbor in neighbors)
//         {
//             if (neighbor.OccupiedCard != null)
//             {
//                 var mod = neighbor.OccupiedCard.GetBehavior<ModifierBehavior>();
//                 if (mod != null)
//                 {
//                     //TODO: Modifiers stack multiplicatively, if additive, change formula below
//                     totalMultiplier *= mod.Multiplier;
//                 }
//             }
//         }
//         
//         return totalMultiplier;
//     }
// }