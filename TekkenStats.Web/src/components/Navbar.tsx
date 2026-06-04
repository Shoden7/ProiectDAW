'use client'
import Link from 'next/link'
import { useAuth } from '@/context/auth-context'

export default function Navbar() {
  const { user, logout, isLoggedIn } = useAuth()

  return (
    <header className="border-b border-iron-800 bg-iron-900/80 backdrop-blur-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
        <Link href="/" className="font-display text-2xl tracking-wider text-gradient-red hover:opacity-90 transition-opacity">
          TEKKEN<span className="text-white">STATS</span>
        </Link>

        <nav className="flex items-center gap-6">
          <Link href="/" className="text-sm font-medium text-iron-200 hover:text-white transition-colors">
            Leaderboard
          </Link>
          
          {/* 🆕 Bookmarks link only visible if user is logged in */}
          {isLoggedIn && (
            <Link href="/bookmarks" className="text-sm font-medium text-gold-light hover:text-gold transition-colors flex items-center gap-1">
              ★ Bookmarks
            </Link>
          )}

          {isLoggedIn ? (
            <div className="flex items-center gap-4">
              <span className="text-xs font-mono text-gold bg-gold/10 px-2 py-1 rounded-sm border border-gold/20">
                {user?.username}
              </span>
              <button onClick={logout} className="btn-ghost py-1.5 px-3 text-xs">
                Logout
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <Link href="/login" className="text-sm font-medium text-iron-300 hover:text-white transition-colors">
                Sign In
              </Link>
              <Link href="/register" className="btn-primary !py-1.5 !px-4 !text-xs">
                Register
              </Link>
            </div>
          )}
        </nav>
      </div>
    </header>
  )
}