export type Product = {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  brand: string;
  type: string;
  quantityInStock: number;
}

// in interface Product
export interface Product1 {
  id: number;
  name: string;
  description: string;
  price: number;
  pictureUrl: string;
  brand: string;
  type: string;
  quantityInStock: number;
  rating: number;
  reviewsCount: number;
  isFavorite: boolean;
}
