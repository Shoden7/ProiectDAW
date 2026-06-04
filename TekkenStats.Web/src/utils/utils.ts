export const CHARACTER_COLORS: Record<string, string> = {
  'Jin': '#1a3a5c', 'Kazuya': '#2a1a4a', 'Devil Jin': '#1a0a2a',
  'Heihachi': '#4a2a0a', 'Paul': '#4a3a0a', 'Law': '#0a3a2a',
  'King': '#2a0a0a', 'Yoshimitsu': '#0a2a1a', 'Hwoarang': '#4a2a1a',
  'Xiaoyu': '#3a1a3a', 'Bryan': '#2a2a2a', 'Steve': '#3a2a1a',
  'Jack-8': '#1a2a3a', 'Asuka': '#3a1a1a', 'Lili': '#3a2a3a',
  'Dragunov': '#1a1a2a', 'Leo': '#2a3a1a', 'Lars': '#1a2a4a',
  'Alisa': '#2a1a3a', 'Claudio': '#3a3a1a', 'Shaheen': '#1a3a1a',
  'Nina': '#3a1a2a', 'Lee': '#2a3a3a', 'Kuma': '#3a2a0a',
  'Panda': '#2a3a2a', 'Zafina': '#2a1a1a', 'Leroy': '#1a1a1a',
  'Jun': '#2a3a4a', 'Reina': '#3a0a2a', 'Azucena': '#3a3a0a',
  'Victor': '#2a2a3a', 'Raven': '#0a0a2a', 'Eddy': '#3a2a2a',
  'Lidia': '#2a2a1a', 'Clive': '#1a2a2a',
}

export const RANK_TIERS: Record<string, { color: string; tier: string }> = {
  'Beginner': { color: '#6b7280', tier: 'beginner' },
  '1st Dan': { color: '#6b7280', tier: 'beginner' },
  '2nd Dan': { color: '#6b7280', tier: 'beginner' },
  '3rd Dan': { color: '#6b7280', tier: 'beginner' },
  'Initiate': { color: '#78716c', tier: 'initiate' },
  'Mentor': { color: '#78716c', tier: 'initiate' },
  'Expert': { color: '#78716c', tier: 'initiate' },
  'Veteran': { color: '#a16207', tier: 'veteran' },
  'Warrior': { color: '#a16207', tier: 'veteran' },
  'Combatant': { color: '#b45309', tier: 'veteran' },
  'Brawler': { color: '#b45309', tier: 'fighter' },
  'Ranger': { color: '#c2410c', tier: 'fighter' },
  'Cavalry': { color: '#c2410c', tier: 'fighter' },
  'Fighter': { color: '#dc2626', tier: 'fighter' },
  'Strategist': { color: '#dc2626', tier: 'fighter' },
  'Dominator': { color: '#dc2626', tier: 'fighter' },
  'Vanquisher': { color: '#b91c1c', tier: 'vanquisher' },
  'Destroyer': { color: '#991b1b', tier: 'vanquisher' },
  'Eliminator': { color: '#7f1d1d', tier: 'vanquisher' },
  'Garyu': { color: '#7c3aed', tier: 'garyu' },
  'Shinryu': { color: '#6d28d9', tier: 'garyu' },
  'Tenryu': { color: '#5b21b6', tier: 'garyu' },
  'Mighty Ruler': { color: '#1d4ed8', tier: 'ruler' },
  'Flame Ruler': { color: '#1d4ed8', tier: 'ruler' },
  'Battle Ruler': { color: '#1e40af', tier: 'ruler' },
  'Fujin': { color: '#0284c7', tier: 'god' },
  'Raijin': { color: '#0369a1', tier: 'god' },
  'Kishin': { color: '#075985', tier: 'god' },
  'Bushin': { color: '#0c4a6e', tier: 'god' },
  'Tekken King': { color: '#c8973a', tier: 'king' },
  'Tekken Emperor': { color: '#c8973a', tier: 'king' },
  'Tekken God': { color: '#e8b45a', tier: 'god-supreme' },
  'Tekken God Supreme': { color: '#e8b45a', tier: 'god-supreme' },
  'God of Destruction': { color: '#ff3040', tier: 'destruction' },
  
  // 🆕 Extended God of Destruction Tiers
  'God of Destruction 1': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 2': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 3': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 4': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 5': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 6': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction 7': { color: '#ff3040', tier: 'destruction' },
  'God of Destruction Infinite': { color: '#e11d48', tier: 'destruction-infinite' },
}

export function getRankColor(rank: string): string {
  if (!rank) return '#6b7280'
  
  const cleanRank = rank.trim();
  
  if (RANK_TIERS[cleanRank]) {
    return RANK_TIERS[cleanRank].color
  }

  // Double check wildcard startsWith criteria
  if (cleanRank.startsWith('God of Destruction')) {
    return cleanRank.includes('Infinite') ? '#e11d48' : '#ff3040'
  }

  return '#6b7280'
}

export function getCharacterBg(character: string): string {
  return CHARACTER_COLORS[character] ?? '#1a1a1f'
}

export function formatWinRate(winRate: number): string {
  return `${winRate.toFixed(1)}%`
}

export function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs}h ago`
  const days = Math.floor(hrs / 24)
  return `${days}d ago`
}/**
 * Returns custom styling configurations or Tailwind CSS utility class strings 
 * depending on the rank tier string.
 */
export function getRankTierClass(rankName: string): string {
  if (!rankName) return 'text-iron-400 bg-iron-950 border-iron-800';

  // 🆕 Support for extended God of Destruction tiers (GoD 1-7 and Infinite)
  if (rankName.startsWith("God of Destruction") || rankName.includes("Infinite")) {
    return 'text-amber-400 font-extrabold tracking-widest border-amber-500 bg-gradient-to-r from-red-950 via-amber-950 to-black animate-pulse shadow-[0_0_15px_rgba(245,158,11,0.3)]';
  }

  // Legacy fallback groupings
  if (rankName.includes("Tekken God") || rankName.includes("Emperor") || rankName.includes("King")) {
    return 'text-yellow-400 font-bold border-yellow-600 bg-iron-950';
  }
  if (rankName.includes("Ruler")) {
    return 'text-purple-400 border-purple-800 bg-iron-950';
  }
  if (rankName.includes("Fujin") || rankName.includes("Raijin") || rankName.includes("Kishin") || rankName.includes("Bushin")) {
    return 'text-blue-400 border-blue-800 bg-iron-950';
  }

  return 'text-iron-300 border-iron-800 bg-iron-950';
}