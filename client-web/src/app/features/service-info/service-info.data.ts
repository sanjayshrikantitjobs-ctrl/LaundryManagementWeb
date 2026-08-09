export interface ServiceInfoCategory {
  slug: string;
  name: string;
  icon: string;
  headline: string;
  description: string;
  comparison: { feature: string; ours: boolean; others: boolean }[];
  benefits: string[];
}

export const SERVICE_INFO_CATEGORIES: ServiceInfoCategory[] = [
  {
    slug: 'laundry',
    name: 'Laundry',
    icon: '🧺',
    headline: 'Reliable everyday laundry, done right every time',
    description:
      'Our standard wash-and-fold process is built around consistency: the right water temperature, the right detergent dosage, and careful sorting by fabric type — so your everyday clothes come back clean, fresh, and exactly the way you handed them in.',
    comparison: [
      { feature: 'Fabric-based sorting before wash', ours: true, others: false },
      { feature: 'Stain pre-treatment included', ours: true, others: false },
      { feature: 'Fragrance-free option available', ours: true, others: false },
      { feature: 'Folded & packed for pickup', ours: true, others: true }
    ],
    benefits: [
      'Separate wash cycles for whites, colors, and delicates',
      'No harsh bleach — fabric-safe detergents only',
      'Every load checked for pockets and loose items before washing'
    ]
  },
  {
    slug: 'dry-cleaning',
    name: 'Dry Cleaning',
    icon: '👔',
    headline: 'Impeccable dry cleaning quality, every time',
    description:
      'Our eco-friendly dry cleaning process is built to protect delicate fabrics, structured garments, and anything with a "dry clean only" label — restoring shine and texture without the shrinkage or color fade you get from harsher solvent-based cleaners.',
    comparison: [
      { feature: '100% garment care label adherence', ours: true, others: false },
      { feature: 'Color-bleed proof process', ours: true, others: false },
      { feature: 'Zero shrinkage guaranteed', ours: true, others: false },
      { feature: '99% stain removal promise', ours: true, others: false }
    ],
    benefits: [
      'Zero cloth shrinkage, 100% preservation of shine and texture',
      'Zero harm to the environment — no toxic oil-based solvents',
      'Specialized treatment for tough India-specific stains (turmeric, henna, ink)'
    ]
  },
  {
    slug: 'shoe-cleaning',
    name: 'Shoe Cleaning',
    icon: '👟',
    headline: 'Bring your sneakers and formal shoes back to life',
    description:
      'From canvas sneakers to leather formals, our shoe cleaning service handles sole scrubbing, upper cleaning, deodorizing, and conditioning — with materials matched to the shoe so nothing gets damaged in the process.',
    comparison: [
      { feature: 'Material-matched cleaning agents', ours: true, others: false },
      { feature: 'Deep sole & midsole scrub', ours: true, others: true },
      { feature: 'Deodorizing treatment included', ours: true, others: false },
      { feature: 'Suede & leather conditioning', ours: true, others: false }
    ],
    benefits: [
      'Safe for canvas, leather, suede, and sports mesh',
      'Odor-neutralizing treatment, not just masking fragrance',
      'Laces cleaned separately to avoid dye transfer'
    ]
  },
  {
    slug: 'carpet-cleaning',
    name: 'Carpet Cleaning',
    icon: '🧶',
    headline: 'Deep-clean carpets without soaking the backing',
    description:
      'Carpets trap dust, allergens, and stains deep in the pile. Our process combines pre-vacuuming, targeted stain treatment, and controlled-moisture washing so the carpet backing stays intact and dries faster.',
    comparison: [
      { feature: 'Pre-vacuum before wet cleaning', ours: true, others: false },
      { feature: 'Controlled moisture (backing-safe)', ours: true, others: false },
      { feature: 'Allergen & dust-mite treatment', ours: true, others: false },
      { feature: 'Pickup & delivery for large carpets', ours: true, others: true }
    ],
    benefits: [
      'Safe for wool, silk-blend, and synthetic carpets',
      'Faster drying thanks to controlled water use',
      'Free pickup for large or heavy carpets'
    ]
  },
  {
    slug: 'leather-cleaning',
    name: 'Leather Cleaning',
    icon: '🧥',
    headline: 'Specialized care for leather jackets, bags, and more',
    description:
      'Leather needs conditioning, not just cleaning. Our leather care service cleans surface dirt and stains, then reconditions the material to prevent cracking — keeping jackets, bags, and accessories supple for longer.',
    comparison: [
      { feature: 'pH-balanced leather cleaners', ours: true, others: false },
      { feature: 'Post-clean conditioning included', ours: true, others: false },
      { feature: 'Color touch-up for scuffed edges', ours: true, others: false },
      { feature: 'Safe for genuine & faux leather', ours: true, others: true }
    ],
    benefits: [
      'Prevents cracking and drying with proper reconditioning',
      'Gentle on stitching and hardware (zips, studs, buckles)',
      'Suitable for jackets, bags, wallets, and upholstery'
    ]
  }
];
