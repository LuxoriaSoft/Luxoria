'use client'

import React from 'react'

interface LuxoriaLogoProps {
  className?: string
}

export const LuxoriaLogo: React.FC<LuxoriaLogoProps> = ({ className }) => (
  <div className="flex items-center gap-2">
    <img
      src="/teams/luxoria.svg"
      alt="Luxoria Logo"
      className={`h-8 ${className ?? ''}`}
    />
    <span className="text-2xl font-bold text-zinc-950 dark:text-white">Luxoria</span>
  </div>
)
