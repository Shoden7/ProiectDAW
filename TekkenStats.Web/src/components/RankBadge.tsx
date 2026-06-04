import React from 'react'
import { getRankTierClass } from '@/utils/utils'

interface RankBadgeProps {
  rankName: string
}

export default function RankBadge({ rankName }: RankBadgeProps) {
  const isHighestTier = rankName.startsWith("God of Destruction") || rankName.includes("Infinite");
  
  return (
    <span className={`text-xs font-mono px-2 py-0.5 border rounded-sm transition-all duration-300 ${getRankTierClass(rankName)}`}>
      {/* Optional: Add a special symbol/icon if the player has transcended past standard limits */}
      {isHighestTier && (
        <span className="text-red-500 mr-1 animate-ping inline-block">⚡</span>
      )}
      {rankName}
    </span>
  )
}