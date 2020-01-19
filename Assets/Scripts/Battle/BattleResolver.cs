using System.Collections.Generic;
using System.Linq;

public static class BattleResolver {

    public static BattleOutcome GetBattleOutcome(List<Card> opUnit, List<Card> plUnit, bool ignoreWizards) {
        if (!ignoreWizards) {
            bool opponentHasWizard = opUnit.Count(card => card.cardData.IsWizard) > 0;
            bool playerHasWizard = plUnit.Count(card => card.cardData.IsWizard) > 0;

            if (opponentHasWizard && playerHasWizard)
                return BattleOutcome.SwapUnitsTwice;
            if (opponentHasWizard || playerHasWizard)
                return BattleOutcome.SwapUnitsOnce;
        }

        bool opponentHasKing = opUnit.Count(card => card.cardData.IsKing) > 0;
        bool playerHasKing = plUnit.Count(card => card.cardData.IsKing) > 0;
        
        bool opponentHasBishop = opUnit.Count(card => card.cardData.IsBishop) > 0;
        bool playerHasBishop = plUnit.Count(card => card.cardData.IsBishop) > 0;
        
        int opponentScore = GetUnitScore(opUnit);
        int playerScore = GetUnitScore(plUnit);

        if (opponentHasKing && !playerHasKing && !playerHasBishop)
            return BattleOutcome.OpponentWins;
        if (playerHasKing && !opponentHasKing && !opponentHasBishop)
            return BattleOutcome.PlayerWins;
        
        if ((opponentHasKing && playerHasKing) || (opponentHasBishop && playerHasBishop)) {
            if (opponentScore > playerScore)
                return BattleOutcome.OpponentWins;
            if (opponentScore < playerScore)
                return BattleOutcome.PlayerWins;
            
            return BattleOutcome.Draw;
        }

        if (opponentHasBishop && playerHasKing)
            return BattleOutcome.OpponentWins;
        if (playerHasBishop && opponentHasKing)
            return BattleOutcome.PlayerWins;

        if (opponentHasBishop)
            return BattleOutcome.PlayerWins;
        if (playerHasBishop)
            return BattleOutcome.OpponentWins;
        
        if (opponentScore > playerScore)
            return BattleOutcome.OpponentWins;
        if (opponentScore < playerScore)
            return BattleOutcome.PlayerWins;
            
        return BattleOutcome.Draw;
    }

    public static int GetUnitScore(List<Card> unit) {
        return unit.Sum(card => card.cardData.Value);
    }
}