import { nanoid } from 'nanoid';

export type CartType = {
  id: string;
  items: CartItem[];
}

export type CartItem = {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  pictureUrl: string;
  brand: string;
  type: string;
}

export class Cart implements CartType {
  // id = ''; // generate a random id in this example nanoid package
  id = nanoid(); // implment nanoid package to generate a random id
  items: CartItem[] = []; // CartItem[] is an array while = [] is an empty array
}
