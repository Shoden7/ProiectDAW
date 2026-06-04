import type { Metadata } from 'next'
import { AuthProvider } from '@/context/auth-context'
import './globals.css'

export const metadata: Metadata = {
  title: 'TekkenStats',
  description: 'Track and analyze Tekken player statistics and matches',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className="relative min-h-screen bg-iron-950 text-iron-100 antialiased overflow-y-auto selection:bg-blood/40 selection:text-white">
        <AuthProvider>
          {/* Main Layout wrapper */}
          <div className="flex flex-col min-h-screen bg-grid-iron bg-[size:30px_30px]">
            {children}
          </div>
        </AuthProvider>
      </body>
    </html>
  )
}