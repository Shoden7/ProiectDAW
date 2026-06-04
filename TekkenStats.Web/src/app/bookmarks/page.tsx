'use client'
import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { api } from '@/services/api'
import { useAuth } from '@/context/auth-context'
import { getRankColor, timeAgo } from '@/utils/utils'
import type { BookmarkDto } from '@/types'
import Navbar from '@/components/Navbar'
import Link from 'next/link'

export default function BookmarksPage() {
  const { isLoggedIn, token } = useAuth()
  const router = useRouter()
  const [bookmarks, setBookmarks] = useState<BookmarkDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // Guard clause: redirect unauthenticated users away
    if (!isLoggedIn && !token) {
      router.push('/login')
      return
    }

    api.bookmarks.list()
      .then((data) => setBookmarks(data))
      .catch((err) => console.error("Could not fetch bookmarks matrix", err))
      .finally(() => setLoading(false))
  }, [isLoggedIn, token, router])

  const handleRemoveBookmark = async (e: React.MouseEvent, playerId: number) => {
    e.preventDefault() // Stop parent click event redirection
    try {
      await api.bookmarks.remove(playerId)
      setBookmarks((prev) => prev.filter((b) => b.playerId !== playerId))
    } catch (err) {
      console.error("Failed to dismiss bookmarked link profile", err)
    }
  }

  if (loading) {
    return (
      <>
        <Navbar />
        <div className="flex-1 flex items-center justify-center font-mono text-xs text-iron-400">
          Syncing secure bookmark monitoring files...
        </div>
      </>
    )
  }

  return (
    <>
      <Navbar />
      <main className="max-w-7xl mx-auto px-4 py-8 w-full flex-1 animate-fade-in">
        <div className="mb-8">
          <h1 className="font-display text-4xl uppercase tracking-wider text-white">Monitored Profiles</h1>
          <p className="text-xs font-mono text-iron-400 uppercase">Your custom shortcut index for verified targets</p>
        </div>

        {bookmarks.length === 0 ? (
          <div className="border border-dashed border-iron-700 bg-iron-900/20 text-center p-12 rounded-sm max-w-xl mx-auto">
            <p className="text-sm font-body text-iron-300 mb-4">No bookmarked profile nodes located inside your roster space.</p>
            <Link href="/" className="btn-primary !text-xs !py-2">Search Global Rosters</Link>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {bookmarks.map((bookmark) => {
              const rankColor = getRankColor(bookmark.currentRank)
              return (
                <Link 
                  key={bookmark.id}
                  href={`/player/${bookmark.playerId}`}
                  className="bg-iron-900 border border-iron-800 hover:border-iron-600 rounded-sm p-5 relative block group transition-all shadow-xl"
                >
                  {/* Left accent color based on character tier status */}
                  <div className="absolute left-0 top-0 h-full w-1" style={{ backgroundColor: rankColor }} />
                  
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="font-bold text-white group-hover:text-blood-glow transition-colors text-lg">
                        {bookmark.playerName}
                      </h3>
                      <p className="text-xs text-iron-500 font-mono mt-0.5">ID: {bookmark.polarisId}</p>
                    </div>
                    <button
                      onClick={(e) => handleRemoveBookmark(e, bookmark.playerId)}
                      className="text-xs text-iron-500 hover:text-blood font-mono uppercase bg-iron-950 px-2 py-1 rounded-sm border border-iron-800 transition-colors"
                    >
                      Dismiss
                    </button>
                  </div>

                  <div className="mt-6 flex justify-between items-center bg-iron-950 p-3 border border-iron-800/80 rounded-sm">
                    <div>
                      <div className="text-[10px] uppercase font-mono text-iron-500">Main Focus</div>
                      <div className="text-xs font-semibold text-gold mt-0.5">{bookmark.mainCharacter || 'Unspecified'}</div>
                    </div>
                    <div className="text-right">
                      <div className="text-[10px] uppercase font-mono text-iron-500">Tier Track</div>
                      <div className="text-xs font-bold mt-0.5" style={{ color: rankColor }}>{bookmark.currentRank}</div>
                    </div>
                  </div>

                  <div className="mt-3 text-[10px] font-mono text-iron-500 text-right">
                    Linked: {timeAgo(bookmark.bookmarkedAt)}
                  </div>
                </Link>
              )
            })}
          </div>
        )}
      </main>
    </>
  )
}